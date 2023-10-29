using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using INNTELEGRAMBOT.Model;

class Program
{
    static Program program = new Program();
    bool isWaitingInn = false;
   
    static void Main(string[] args)
    {
        string path = @"C:\apis\telegram\inntestapi.txt";

        string api;

        using (StreamReader reader = new StreamReader(path))
        {
            api =  reader.ReadToEnd();    
        }

        var client = new TelegramBotClient(api);

        client.StartReceiving(Update, Error);

        Console.ReadLine();
    }

    private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    {
                        var message = update.Message;

                        switch (message.Type)
                        {
                            case MessageType.Text:
                                {
                                    if (program.isWaitingInn)
                                    {
                                        string[] inns = message.Text.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                                        foreach (string inn in inns)
                                        {
                                            if (inn.Length == 10 && long.TryParse(inn, out _))
                                            {
                                                await program.SearchCompanyByInnAndSendResult(botClient, message.Chat.Id, inn);
                                            }
                                            else
                                            {
                                                // Обработка некорректного ввода ИНН
                                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Некорректный ИНН: {inn}");
                                            }
                                        }

                                        program.isWaitingInn = false;
                                    }
                                    else
                                    {
                                        if (message.Text == "/start")
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                "Добро пожаловать, бот находит информацию о компании по ИНН.\n" +
                                                "Список доступных команд:\n" +
                                                "/help\n"
                                                );
                                        }
                                        if (message.Text == "/help")
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                "Cписок доступных команд:\n" +
                                                "/start - Запуск бота \n" +
                                                "/hello - Информация о создателе бота\n" +
                                                "/help - Список доступных команд\n" +
                                                "/inn - Поиск информации о компании по ИНН"
                                                );
                                        }

                                        if (message.Text == "/hello")
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                "Максим Петров\n" +
                                                "max.petrv15@gmail.com\n" +
                                                "Задание получено 28.10.2023"
                                                );
                                        }

                                        if (message.Text == "/inn")
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите ИНН компании (10 цифр) или несколько ИНН, разделённых запятой или пробелом:");
                                            program.isWaitingInn = true;
                                        }
                                    }
                                    return;
                                }
                        }
                        return;
                    }
                default:
                    return;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private async Task<RootObject?> ConnectToApiService(string inn)
    {
        string ApiKey;
        string path = @"C:\apis\fns\fnsapi.txt";

        using (StreamReader reader = new StreamReader(path))
        {
            ApiKey = reader.ReadToEnd();
        }


        string url = $"https://api-fns.ru/api/search?q={inn}&filter=active&key={ApiKey}";

        using (HttpClient client = new HttpClient())
        {

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseText = await response.Content.ReadAsStringAsync();

                RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(responseText);

                return rootObject;
            }
            catch (HttpRequestException e)
            {
                // Обработка ошибок
                Console.WriteLine("Ошибка при выполнении HTTP-запроса: " + e.Message);
                return null;
            }

        }
    }

    private async Task SearchCompanyByInnAndSendResult(ITelegramBotClient botClient, long chatId, string inn)
    {
        RootObject? companyInfo = await program.ConnectToApiService(inn);

        if (companyInfo != null && companyInfo.items.Count > 0)
        {
            // Поиск компании с нужным ИНН
            var targetCompany = companyInfo.items.FirstOrDefault(item => item.ЮЛ.ИНН == inn);

            if (targetCompany != null)
            {
                var company = targetCompany.ЮЛ;

                string responseText = $"ИНН: {company.ИНН}\n" +
                                    $"ОГРН: {company.ОГРН}\n" +
                                    $"Наименование: {company.НаимПолнЮЛ}\n" +
                                    $"Полный адрес: {company.АдресПолн}\n" +
                                    $"Основной вид деятельности: {company.ОснВидДеят}\n" +
                                    $"Статус: {company.Статус}\n";

                await botClient.SendTextMessageAsync(chatId, responseText);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Информация о компании не найдена.");
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Информация не найдена.");
        }
    }
}