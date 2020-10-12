using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json.Linq;
using Google.Cloud.Dialogflow.V2;
using Newtonsoft.Json;


namespace TelegramBot {
    class Program {
        private static TelegramBotClient Bot;       // телеграм-бот
        private static SessionsClient dFlowClient;  // DialogFlow-клиент
        private static string projectID;            // идентификатор проекта (DialogFlow)
        private static string sessionID;            // идентификатор сессии (DialogFlow)

        private static List<string> cities = File.ReadAllText(@"C:\SKILLBOX_STUDY\C#\HOMEWORK\9\TelegramBot\Data_Files\WorldCities.txt").Split("\r\n").ToList();  // список городов
        private static AllGames games;      // все текущие игры с ботом в города


        private static Dictionary<string, string> menu = new Dictionary<string, string> {
            ["Story"] = "Расскажи сказку!",
            ["Sities"] = "Поиграем в города."
        };

        static void Main(string[] args) {

            string token = File.ReadAllText(@"C:\SKILLBOX_STUDY\C#\HOMEWORK\9\TelegramBot\Data_Files\tokens\token");                    // токен для бота
            string dFlowKeyPath = @"C:\SKILLBOX_STUDY\C#\HOMEWORK\9\TelegramBot\Data_Files\tokens\small-talk-rghy-1fa31b152405.json";   // путь к токену для DialogFlow бота

            // Создание telegram-бота
            Bot = new TelegramBotClient(token);

            // Создание DialogFlow клиента
            var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(dFlowKeyPath));

            projectID = dic["project_id"];
            sessionID = dic["private_key_id"];

            var dialogFlowBuilder = new SessionsClientBuilder {
                CredentialsPath = dFlowKeyPath
            };

            dFlowClient = dialogFlowBuilder.Build();

            games = new AllGames();

            // Обрабатывем сообщения от пользователя бота
            Bot.OnMessage += Bot_OnMessage;
            Bot.OnCallbackQuery += Bot_OnCallbackQuery;

            Bot.StartReceiving();

            Console.ReadKey();

            Bot.StopReceiving();
        }


        /// <summary>
        /// Обработчик событий нажатия от пользователя
        /// </summary>
        /// <param name="sender">Объект отправивший сигнал</param>
        /// <param name="e">Событие отправки сигнала</param>
        private static async void Bot_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e) {
            string buttonText = e.CallbackQuery.Data;

            if (buttonText == menu["Story"]) {
                // Выбран рассказ сказки
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, returnFairyTale(@"C:\SKILLBOX_STUDY\C#\HOMEWORK\9\TelegramBot\Data_Files\FairyTales.json"));
                
            }

            if (buttonText == menu["Sities"]) {
                // Выбрана игра в города
                string answerText = "<b>ГОРОДА.</b>\n" +
                                    "<i>В игре учавствуют названия городов, состоящие из одного слова и не включающие в себя знак '-'.\n" +
                                    "Каждый игрок по очереди пишет название города, начинающееся с той буквы на которую заканчивалось название предыдущего города." +
                                    "Проигрывает тот, кто напишет 'конец'.</i>";

                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, answerText, parseMode: ParseMode.Html);

                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Теперь можешь назвать город.");

                long chatId = e.CallbackQuery.Message.Chat.Id;

                List<string> copyCities = new List<string>();
                copyCities.AddRange(cities);    // делаем копию списка городов, чтобы источник не менялся

                games.addGame(new CitiesGame(chatId, copyCities));  // добавляет новую игру в города с ботом

            }

        }


        /// <summary>
        /// Обработчик сообщения боту
        /// </summary>
        /// <param name="sender">Объект отправивший сигнал</param>
        /// <param name="e">Событие отправки сообщения</param>
        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e) {

            var message = e.Message;

            if (message == null) return;        // если сообщение null выходим из метода

            long chatId = message.Chat.Id;  // идентификатор чата

            // Получение и сохранение ботом фото, аудио, видео и документа (и отправка их назад пользователю)
            switch (message.Type) {
                case MessageType.Photo: // пользователь прислал фото
                    string fileNamePhoto = message.Photo[message.Photo.Length - 1].FileUniqueId + ".jpeg";  // имя файла фото
                    string fileIdPhoto = message.Photo[message.Photo.Length - 1].FileId;                    // идентификатор файла фото

                    // Последнее фото в массиве PhotoSize является оригиналом
                    DownLoad(fileIdPhoto, fileNamePhoto);

                    // Отправка фото обратно
                    await Bot.SendPhotoAsync(chatId, fileIdPhoto);

                    break;

                case MessageType.Audio: // пользователь прислал аудио
                    string fileNameAudio = message.Audio.Title + "." + message.Audio.MimeType.Split("/")[1];    // имя файла аудио
                    string fileIdAudio = message.Audio.FileId;                                                  // идентификатор файла аудио

                    DownLoad(fileIdAudio, fileNameAudio);

                    // Отправка аудио обратно
                    await Bot.SendAudioAsync(chatId, fileIdAudio);

                    break;

                case MessageType.Video: // пользователь прислал видео
                    string fileNameVideo = message.Video.FileUniqueId + "." + message.Video.MimeType.Split("/")[1]; // имя файла видео
                    string fileIdVideo = message.Video.FileId;                                                      // идентификатор файла видео

                    DownLoad(fileIdVideo, fileNameVideo);

                    // Отправка аудио обратно
                    await Bot.SendVideoAsync(chatId, fileIdVideo);

                    break;

                case MessageType.Document:  // пользователь прислал документ
                    string fileNameDocument = message.Document.FileName;    // имя файла документа
                    string fileIdDocument = message.Document.FileId;        // идентификатор файла документа

                    DownLoad(fileIdDocument, fileNameDocument);

                    // Отправка аудио обратно
                    await Bot.SendDocumentAsync(chatId, fileIdDocument);

                    break;


                case MessageType.Text:
                    // Если чат пользователя в игре, то играем
                    if (games.isFindId(chatId)) {
                        string city = message.Text; // город, который отправил пользователь
                        
                        // Если пользователь решил выйти из игры
                        if (city.ToUpper() == "КОНЕЦ") {
                            games.removeGame(chatId);   // удаляем игру
                            return;
                        }

                        Console.WriteLine($"Играет пользователь {message.Chat.Username} с идентификатором чата: {chatId}");

                        // Если город есть в "википедии" бота
                        if (games.returnGame(chatId).isWikiKnowCity(city)) {
                            string lastCityBot = games.returnGame(chatId).LastCityBotSay;   // предыдущий названный ботом город

                            // Если бот уже называл город
                            if (!String.IsNullOrEmpty(lastCityBot)) {

                                if (char.ToUpper(lastCityBot[lastCityBot.Length - 1]) == 'Ь') {
                                    if (char.ToUpper(lastCityBot[lastCityBot.Length - 2]) != char.ToUpper(city[0])) {
                                        await Bot.SendTextMessageAsync(chatId, $"Город должен начинаться на букву '{lastCityBot[lastCityBot.Length - 2]}'!");
                                        return;
                                    }

                                }
                                else {
                                    if (char.ToUpper(lastCityBot[lastCityBot.Length - 1]) != char.ToUpper(city[0])) {
                                        await Bot.SendTextMessageAsync(chatId, $"Город должен начинаться на букву '{lastCityBot[lastCityBot.Length - 1]}'!");
                                        return;
                                    }

                                }
                            }


                            games.returnGame(chatId).delCitiyInWikiBOT(city);   // удаляем город из общего списка городов ("википедии бота"), чтобы город не повторялся
                            games.returnGame(chatId).delCityInKnowBOT(city);    // удаляем город из "базы знаний" бота

                            // Если город оканчивается на мягкий знак, то бот ищет в своей "базе знаний" город начинающийся на предпоследнюю букву
                            if (city[city.Length - 1] == 'ь')
                                city = games.returnGame(chatId).cityWhoBotKnow(city[city.Length - 2]);
                            else
                                city = games.returnGame(chatId).cityWhoBotKnow(city[city.Length - 1]);

                            // Если бот не может дать ответ то завершаем игру
                            if (String.IsNullOrEmpty(city)) {
                                await Bot.SendTextMessageAsync(chatId, "Молодец, выигрышь за тобой!");
                                await Bot.SendTextMessageAsync(chatId, "конец");

                                games.removeGame(chatId);   // удаляем игру
                                return;
                            }

                            games.returnGame(chatId).LastCityBotSay = city; // последний названный ботом город (для проверки следующего города, введенного пользователем)

                            string htmlCity = $"<i>{city}</i>";
                            await Bot.SendTextMessageAsync(chatId, htmlCity, parseMode: ParseMode.Html);

                            games.returnGame(chatId).delCitiyInWikiBOT(city);   // удаляем город из общего списка городов ("википедии бота"), чтобы город не повторялся
                            games.returnGame(chatId).delCityInKnowBOT(city);    // удаляем город из "базы знаний" бота

                        } else {
                            // Если города нет в "википедии" бота, либо его уже называли
                            await Bot.SendTextMessageAsync(chatId, $"Город '{city}' моя Википедия не знает! Возможно этот город уже называли.");

                            return;
                        }

                        return;
                    }

                    break;

                default:

                    break;

            }


            if (message.Text == null) return;   // если текст сообщения null выходим из метода

            // Сообщение от бота (в формате HTML)
            var answerText =    "Меня зовут Сказочник.\nЯ люблю общаться с людьми, рассказывать разные сказки и играть в 'Города'!\n\n" +
                                "<b>Выбери команду:</b>\n" +
                                "/start - <i>запуск бота</i>\n" +
                                "/menu - <i>вывод меню</i>";

            switch (message.Text) {
                case "/start":
                    await Bot.SendTextMessageAsync(chatId, answerText, ParseMode.Html); // вывод начального сообщения

                    break;

                case "/menu":
                    // Создаем меню (клавиатуру)
                    var inlineKeyboard = new InlineKeyboardMarkup(new[] {
                        new[] { InlineKeyboardButton.WithCallbackData(menu["Story"]) },
                        new[] { InlineKeyboardButton.WithCallbackData(menu["Sities"]) }
                    });

                    await Bot.SendTextMessageAsync(chatId, "<b>Чем займемся?</b>", parseMode: ParseMode.Html, replyMarkup: inlineKeyboard);

                    break;

                default:
                    // Общение с ботом через DialogFlow
                    // Инициализируем аргументы ответа
                    SessionName session = SessionName.FromProjectSession(projectID, sessionID);
                    var queryInput = new QueryInput {
                        Text = new TextInput {
                            Text = message.Text,
                            LanguageCode = "ru-ru"
                        }
                    };

                    // Создаем ответ пользователю
                    DetectIntentResponse response = await dFlowClient.DetectIntentAsync(session, queryInput);

                    answerText = response.QueryResult.FulfillmentText;

                    if (answerText == "") {
                        // Intents для непонятных боту фраз
                        queryInput.Text.Text = "непонятно";
                    }

                    // Создаем ответ пользователю, если введен непонятный вопрос (набор букв)
                    response = await dFlowClient.DetectIntentAsync(session, queryInput);

                    answerText = response.QueryResult.FulfillmentText;

                    await Bot.SendTextMessageAsync(chatId, answerText); // отправляем пользователю ответ

                    break;


            }

        }

        /// <summary>
        /// Возвращает случайную сказку
        /// </summary>
        /// <param name="path">Путь к json-файлу со сказками</param>
        /// <returns>Строка со сказкой</returns>
        private static string returnFairyTale(string path) {
            string json = File.ReadAllText(path);
            Random rnd = new Random(DateTime.Now.Millisecond);
            
            var jsonStories = JObject.Parse(json)["stories"].ToArray();
            int index = rnd.Next(0, jsonStories.Length);

            return jsonStories[index].ToString();
        }


        /// <summary>
        /// Сохраняет переданный пользователем файл по его идентификатору
        /// </summary>
        /// <param name="fileId">Идентификатор переданного файла</param>
        /// <param name="path">Путь по которому сохраняем файл</param>
        private static async void DownLoad(string fileId, string path) {
            var file = await Bot.GetFileAsync(fileId);

            FileStream fs = new FileStream(path, FileMode.Create);

            await Bot.DownloadFileAsync(file.FilePath, fs);

            fs.Close();
            fs.Dispose();
        }
    }
}
