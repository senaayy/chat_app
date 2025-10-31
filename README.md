# C# TCP Soket ile Çok İstemcili Sohbet Sunucusu

Bu proje, C# dilinde ve .NET platformunda geliştirilmiş basit bir TCP soket sunucusu konsol uygulamasıdır. Sunucu, birden fazla istemcinin (client) aynı anda bağlanmasına ve birbirlerine gerçek zamanlı olarak mesaj göndermesine olanak tanır.

---

##  Kullanılan Teknolojiler

* **C#** (.NET 7.0 / .NET 6.0)
* **`System.Net.Sockets`** Kütüphanesi (`TcpListener`, `TcpClient`)
* **Asenkron Programlama** (`async/await`, `Task`)

---

## Nasıl Çalışır?

1.  **Sunucu Başlatma:** `Program.cs` içinde `TcpListener` sınıfı, belirtilen portu (örn: `9000`) dinlemeye başlar.
2.  **İstemci Kabul Etme:** Sunucu, `while(true)` döngüsü içinde yeni istemci bağlantılarını `AcceptTcpClientAsync()` ile asenkron olarak bekler.
3.  **Çoklu Görev (Multithreading):** Her yeni istemci bağlandığında, o istemci için `HandleClientAsync` adında yeni bir `Task` (görev) başlatılır. Bu, sunucunun aynı anda birden fazla istemciyi yönetebilmesini (non-blocking) sağlar.
4.  **Mesaj Yayını (Broadcast):** Bir istemciden bir mesaj geldiğinde (`StreamReader.ReadLineAsync`), sunucu bu mesajı alır ve o an bağlı olan **diğer tüm istemcilere** geri yollar.
5.  **Bağlantı Kesilmesi:** Bir istemci bağlantısı kesildiğinde, sunucu bu durumu algılar, o istemciyi bağlı listesinden çıkarır ve diğer kullanıcılara "Kullanıcı ayrıldı" mesajı gönderir.

---

##  Çalıştırma Adımları

### 1. Sunucuyu Başlatma

1.  Projeyi Visual Studio'da açın.
2.  (Opsiyonel) `Program.cs` dosyasını açarak `PORT` değişkenini istediğiniz bir port numarasıyla değiştirin (örn: `private const int PORT = 9000;`).
3.  Projeyi (Yeşil 'Start' ‣ butonu ile) çalıştırın.
4.  Konsol ekranında "Sunucu başlatıldı, istemciler bekleniyor..." gibi bir mesaj göreceksiniz.

### 2. Sunucuya Bağlanma (Test için)

Bu proje **sadece sunucuyu** içerir. Bağlanmak için bir istemciye (client) ihtiyacınız vardır.

**Test için `netcat` veya `telnet` kullanabilirsiniz:**

1.  Bilgisayarınızda (veya ağdaki başka bir bilgisayarda) yeni bir terminal (Komut İstemi) açın.
2.  Sunucunuzun çalıştığı IP adresine ve porta bağlanın. Eğer sunucuyla aynı makinedeyseniz:

    ```bash
    telnet 127.0.0.1 9000
    ```
    *(`9000`, `Program.cs` içinde belirlediğiniz port numarası olmalıdır)*

3.  Bu komutu **birden fazla** (2-3 tane) terminal penceresinde çalıştırarak sunucuya birden çok istemci bağlayabilirsiniz.
4.  Bir terminale yazdığınız her şeyin, sunucu üzerinden diğer terminallere de iletildiğini göreceksiniz.
