// Program.cs - İstemci Tarafı
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class Client
{
    public static async Task Main(string[] args)
    {
        // Sunucuya bağlanmak için TcpClient nesnesi oluştur.
        // IP ve Port, sunucunun çalıştığı adresle aynı olmalı.
        string serverIp = "127.0.0.1";
        int port = 8888;
        TcpClient client = new TcpClient();

        try
        {
            await client.ConnectAsync(serverIp, port);
            Console.WriteLine("Sunucuya bağlandı!");

            Console.Write("Lütfen kullanıcı adınızı girin: ");
            string username = Console.ReadLine();
            Console.WriteLine($"Hoş geldin, {username}! Sohbete başlayabilirsiniz (çıkış için 'exit' yazın).");

            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            // Sunucudan gelen mesajları dinlemek için ayrı bir Task başlat.
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var messageFromServer = await reader.ReadLineAsync();
                        if (messageFromServer == null)
                        {
                            Console.WriteLine("Sunucu bağlantısı koptu.");
                            break;
                        }
                        Console.WriteLine(messageFromServer);
                    }
                    catch
                    {
                        Console.WriteLine("Sunucuyla bağlantı kesildi.");
                        break;
                    }
                }
            });

            // Kullanıcıdan mesaj almak ve sunucuya göndermek için ana döngü.
            string messageToSend;
            while ((messageToSend = Console.ReadLine()) != null)
            {
                if (messageToSend.ToLower() == "exit")
                {
                    break;
                }
                // Mesajı "KullanıcıAdı: Mesaj" formatında gönder.
                await writer.WriteLineAsync($"{username}: {messageToSend}");
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Sunucuya bağlanılamadı. Sunucunun çalıştığından emin olun.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bir hata oluştu: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine("Bağlantı kapatıldı.");
        }
    }
}