# Kurulum

Solution açıldıktan sonra;  Services klasörü altında bulunan ContactAPI ve ReportAPI projelerinin appsettings.json dosyaları içerisinde ki postgresql ve rabbitmq bağlantı bilgileri değiştirilir. Ayrıca aynı işlemler APIGateway projesi içinde yapılır.


# Migration Kurulum

Solution'ı çalıştırdıktan sonra Package Manager Console ekranında Current Project: Shared.Data projesi seçilerek Update-Database komutu çalıştıralarak veritabanı ve tablolar oluşturulur.

# Çalıştırma

Solution'ı açtıktan sonra Solution üzerine sağ tıklayarak Set Startup Projects tıklanır. Açılan ekranda Multiple startup projects seçilerek sıralamasını ve Actions'ı aşağıdaki gibi değiştiriliri.
ContactAPI - Start 
ReportAPI  - Start 
APIGateway - Start seçilir.

# Kullanma

APIGateway üzerinde route işlemleri için Ocelot kullanılmıştır. Gateway üzerinden requestleri rahat kullanabilmek için proje dizinindeki postman collection requestlerini kullanabilirsiniz.
Ayrıca ContactAPI ve ReportAPI kendi içlerinde swagger entegrasyonu bulunmaktadır. Swagger üzerinden manuel bir şekilde hızlıca test edilebilir. 
