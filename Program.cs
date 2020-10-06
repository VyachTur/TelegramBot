using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;


namespace TelegramBot {
    class Program {
        static TelegramBotClient Bot;

        static void Main(string[] args) {

            string token = File.ReadAllText(@"C:\SKILLBOX_STUDY\C#\HOMEWORK\9\TelegramBot\token");  // токен для бота
            string[] cities = File.ReadAllText(@"C:\SKILLBOX_STUDY\C#\HOMEWORK\9\TelegramBot\WorldCities.txt").Split('\n');

            Console.WriteLine(cities[6]);

            Bot = new TelegramBotClient(token);
            Bot.OnMessage += Bot_OnMessage;
            Bot.StartReceiving();

            Console.ReadKey();

            Bot.StopReceiving();
        }


        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e) {

            var message = e.Message;

            if (message == null) return;        // если сообщение null выходим из метода

            string text = $"{DateTime.Now.ToLongTimeString()}: {message.Chat.FirstName} {message.Chat.Id} {message.Text}";

            Console.WriteLine($"{text} TypeMessage: {message.Type.ToString()}");


            //if (message.Type == Telegram.Bot.Types.Enums.MessageType.Document) {
            //    Console.WriteLine(message.Document.FileId);
            //    Console.WriteLine(message.Document.FileName);
            //    Console.WriteLine(message.Document.FileSize);
            //    Console.WriteLine(message.Document.MimeType);
            //    Console.WriteLine(message.Document.Thumb);

            //DownLoad(message.Document.FileId, message.Document.FileName);
            //}

            switch (message.Type) {
                case MessageType.Photo:
                    // Последнее фото в массиве PhotoSize является оригиналом
                    DownLoad(message.Photo[message.Photo.Length - 1].FileId, message.Photo[message.Photo.Length - 1].FileUniqueId + ".jpeg");

                    break;

                case MessageType.Audio:
                    DownLoad(message.Audio.FileId, message.Audio.Title +
                                            "." + message.Audio.MimeType.Split("/")[1]);

                    break;

                case MessageType.Video:
                    DownLoad(message.Video.FileId, message.Video.FileUniqueId +
                                            "." + message.Video.MimeType.Split("/")[1]);

                    break;

                case MessageType.Document:
                    DownLoad(message.Document.FileId, message.Document.FileName);

                    break;

                default:

                    break;

            }


            if (message.Text == null) return;   // если текст сообщения null выходим из метода

            switch (message.Text) {
                case "/start":

                    break;

                case "/menu":

                    break;

                case "/game":

                    break;

                default:

                    var messageText = message.Text;

                    await Bot.SendTextMessageAsync(message.Chat.Id, $"{messageText}");


                    break;


            }

            

        }


        static async void DownLoad(string fileId, string path) {
            var file = await Bot.GetFileAsync(fileId);

            FileStream fs = new FileStream(path, FileMode.Create);

            await Bot.DownloadFileAsync(file.FilePath, fs);

            fs.Close();
            fs.Dispose();
        }
    }
}
