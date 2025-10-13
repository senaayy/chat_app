// Program.cs - Sunucu Tarafı (DÜZELTİLMİŞ)
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class Server
{
    // Bağlı olan tüm istemcileri (ve onların streamlerini) saklamak için bir sözlük.
    private static Dictionary<TcpClient, StreamWriter> clients = new Dictionary<TcpClient, StreamWriter>();
    private static readonly object lockObj = new object(); // Thread-safe işlemler için kilit nesnesi

    public static async Task Main(string[] args)
    {
        // Sunucuyu 127.0.0.1 (localhost) IP adresi ve 8888 portunda başlat.
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        int port = 8888;
        TcpListener listener = new TcpListener(ip, port);

        try
        {
            listener.Start();
            Console.WriteLine($"Sunucu başlatıldı. Port: {port}");
            Console.WriteLine("İstemci bağlantıları bekleniyor...");

            // Sürekli olarak yeni istemci bağlantılarını kabul et.
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                // Yeni bir istemci bağlandığında, onu ayrı bir iş parçacığında (thread) ele al.
                // Bu sayede sunucu aynı anda birden fazla istemciye hizmet verebilir.
                Task.Run(() => HandleClient(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
        finally
        {
            listener.Stop();
        }
    }

    private static async void HandleClient(TcpClient tcpClient)
    {
        StreamWriter sWriter = null;
        string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
        try
        {
            NetworkStream stream = tcpClient.GetStream();
            StreamReader sReader = new StreamReader(stream);
            sWriter = new StreamWriter(stream) { AutoFlush = true };

            // Thread-safe bir şekilde yeni istemciyi listeye ekle.
            lock (lockObj)
            {
                clients.Add(tcpClient, sWriter);
            }

            // Bağlantı mesajını herkese duyur.
            Console.WriteLine($"İstemci bağlandı: {clientEndPoint}");
            await BroadcastMessage($"[Sunucu]: {clientEndPoint} sohbete katıldı.");

            // İstemciden sürekli olarak mesaj oku.
            while (true)
            {
                string message = await sReader.ReadLineAsync();

                // Eğer mesaj null ise, istemcinin bağlantısı kopmuş demektir.
                if (message == null)
                {
                    break; // Döngüyü kır ve istemciyi kaldır.
                }

                Console.WriteLine($"Gelen Mesaj ({clientEndPoint}): {message}");
                // Gelen mesajı, gönderen dahil herkese yayınla.
                await BroadcastMessage(message);
            }
        }
        catch (Exception)
        {
            // Genellikle istemci aniden kapandığında bu hatayı alırız.
        }
        finally
        {
            // Hata oluştuğunda veya istemci ayrıldığında istemciyi listeden kaldır.
            lock (lockObj)
            {
                clients.Remove(tcpClient);
            }
            Console.WriteLine($"İstemci ayrıldı: {clientEndPoint}");
            await BroadcastMessage($"[Sunucu]: {clientEndPoint} sohbetten ayrıldı.");
            tcpClient.Close();
        }
    }

    // Gelen bir mesajı tüm istemcilere gönderen metot.
    // ***** BURASI DÜZELTİLDİ *****
    private static async Task BroadcastMessage(string message)
    {
        List<StreamWriter> currentWriters;

        // lock'un etki alanını küçültüyoruz.
        // Sadece istemci listesinin anlık bir kopyasını almak için kullanıyoruz.
        lock (lockObj)
        {
            currentWriters = new List<StreamWriter>(clients.Values);
        }

        // Artık kilitli alanın dışındayız ve asenkron işlemleri güvenle yapabiliriz.
        // O anki istemcilerin kopyası üzerinde döngü kuruyoruz.
        foreach (var writer in currentWriters)
        {
            try
            {
                await writer.WriteLineAsync(message);
            }
            catch (Exception)
            {
                // Mesaj gönderilemeyen bir istemci olursa (bağlantı kopmuş olabilir),
                // o istemcinin kendi HandleClient metodu bu durumu zaten yakalayıp
                // gerekli temizliği yapacağı için burada ek bir işlem yapmaya gerek yok.
            }
        }
    }
}