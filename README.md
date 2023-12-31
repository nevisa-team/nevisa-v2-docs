<!-- language: rtl -->
# لیست سرویس‌های اپلیکیشن:

## 1. api-server

- این سرویس به درخواست‌های تمامی کاربران به صورت همزمان پاسخ می‌دهد
- مستندات `Swagger` نحوه استفاده از `API` نیز موجود است. `https://<ServerAddress>/api/docs`
- مدیریت کاربران و اتصال آنها از طریق Websocket (Socket.io) به عهده این سرویس است

## 2. asr-server
- تبدیل گفتار به متن به زبان فارسی و استخراج کلمات کلیدی به عهده این سرویس می‌باشد
## 3. queue-server
- مدیریت پردازش فایل‌های هر کاربر و تشکیل صف انتظار برای هر کاربر به عهده این سرویس می‌باشد

# سرویس PM2

تمامی سرویس‌های بالا با استفاده از `PM2` مدیریت می‌شود. مستندات کامل استفاده از `PM2` در این [لینک](https://pm2.keymetrics.io/docs/usage/quick-start/) موجود است.

تمامی دستورات `PM2` حتماً باید با `sudo` اجرا شود.


- پس اجرای این دستور لیست تمامی سرویس‌ها به همراه جزئیات نمایش داده می‌شود
```console
sudo pm2 ls
```
<div style="margin-left: auto; margin-right: auto; width: 70%">
  
| id | name | mode | ↻ | status | cpu | memory |
| --- | --- | --- |---|---|---|---|
| 0 | api-server | cluster | 8 | $${\color{green}online}$$ | 0% | 91.7mb |
| 1 | asr-server | fork | 754 | $${\color{green}online}$$ | 0% | 3.9mb |
| 2 | queue-server | cluster | 2 | $${\color{green}online}$$ | 0% | 70.7mb |

</div>

- پس از اجرای دستور سرویس مورد نظر / همه سرویس‌ها راه اندازی خواهد شد
```console
sudo pm2 start <Service Name>
```
- پس از اجرای دستور سرویس مورد نظر متوقف خواهد شد
```console
sudo pm2 stop <Service Name>
```
- پس از اجرای دستور سرویس مورد نظر راه اندازی خواهد شد
```console
sudo pm2 restart <Service Name>
```
-  پس از اجرای دستور سرویس مورد نظر راه اندازی خواهد شد
```console
sudo pm2 delete <Service Name>
```
- پس از اجرای دستور وضعیت فعلی سرویس‌ها ذخیره خواهد شد. پس از راه اندازی مجدد سیستم عامل، سرویس‌ها با آخرین وضعیت ذخیره شده اجرا می‌شوند
```console
sudo pm2 save
```

# اتصال با Socket.io

## نحوه اتصال به socket در Javascript:

با استفاده از دستور زیر پکیج `socket.io` رانصب کنید

```console
npm install socket.io-client
```

```js
import { io } from "socket.io-client";
const socket = io("SERVER_IP_ADDRESS_OR_URL", { extraHeaders: { token: token, platform: "mobile-app" || "browser" }})
```

توکن: از سرویس `Swagger` استفاده کنید و در قسمت `Authentication` درخواست `Login` را ارسال کنید. در جواب درخواست، توکن برای شما ارسال خواهد شد.

## نحوه ارسال و دریافت پیام به سوکت:
```js
socket.emit("Your Event Name", data)
socket.on("Your Event Listener Name", (response) => {
console.log(response); //socket response
})
```


## لیست رویدادهای سوکت (Events):

1. get-online-users
- پس از ارسال پیام به این event، لیست کاربران آنلاین به یک event listener به نام online-users ارسال می‌شود
- این event فقط از طریق کاربرانی که مدیر هستند قابل ارسال است در غیر اینصورت پاسخی دریافت نمی‌کنید

2. disconnect-user
- پس از ارسال این event کاربر مورد نظر از حساب کاربری خود خارج می‌شود و پیام مربوطه به کاربر نمایش داده می‌شود
- این event فقط از طریق کاربرانی که مدیر هستند قابل ارسال است در غیر اینصورت پاسخی دریافت نمی‌کنید
- ارسال داده حتماً باید به صورت JSON باشد مثال {id: "user id", deleteUser: true/false}
- درصورتی که کاربر مورد نظر حذف کرده باشید باید مقدار deleteUser را true و صورتی که حساب کاربری شخص مورد نظر را مسدود کرده‌اید مقدار را false قرار دهید

3. get-system-monitor
- پس از ارسال پیام به این event، لیست کاربران آنلاین به یک event listener به نام system-monitor ارسال می‌شود
- این event فقط از طریق کاربرانی که مدیر هستند قابل ارسال است در غیر اینصورت پاسخی دریافت نمی‌کنید

4. start-microphone
- با ارسال این event سرور، برای دریافت chunk های صوتی آماده می‌شود
- پس از ارسال، سرور، قفل سخت افزاری را بررسی می‌کند. این مرحله ممکن از زمانبر باشد.
- نتیجه بررسی قفل سخت افزاری در یک event listener به نام start-microphone گزارش خواهد شد

5. microphone-blob
- پس از ارسال event مربوط به آماده سازی سرور (start-microphone) و دریافت گزارش بررسی قفل سخت افزاری درصورتی که گزارش lockChecked: true بود می‌توانید chunk های صوتی را ارسال کنید
- برای بهبود در سرعت دریافت نتیجه chunk های صوتی را هر 500 میلی ثانیه ارسال کنید
- نتیجه در یک event listener به نام result برای شما ارسال خواهد شد

6. stop-microphone
- پس از ارسال event پردازش شما میکروفونی متوقف خواهد شد

7. set-text-to-number
- با ارسال این event، سرویس تبدیل اعداد فعال/غیرفعال خواهد شد
- ارسال داده حتماً باید به صورت JSON باشد مثال { textToNumberStatus: true / false}
- درصورت بروز خطا، پیام خطا در یک event listener به نام message گزارش خواهد شد

8. set-punctuations
- با ارسال این event، سرویس درج خودگار علائم نگارشی فعال/غیرفعال خواهد شد
- ارسال داده حتماً باید به صورت JSON باشد مثال { punctuationStatus: true / false}
- درصورت بروز خطا، پیام خطا در یک event listener به نام message گزارش خواهد شد

9. start-file-process
- پس از ارسال فایل با استفاده از API، اطلاعات فایل یا فایل‌های ارسال شده را به صورت JSON Array در این event ارسال کنید
- پس از ارسال این event قفل سخت افزاری بررسی و نتیجه در یک event listener به نام start -file-process برای شما ارسال خواهد شد
- درصورت بروز خطا، اطلاعات خطا در یک event listener به نام message برای شما ارسال خواهد شد
- درصورت عدم بروز خطا، پردازش فایل آغاز خواهد شد و آخرین وضعیت فایل مورد نظر در یک event listener به نام queue-report برای شما ارسال خواهد شد

10. stop-file-process
- با ارسال شناسه فایل در حال پردازش، عملیات پردازش فایل مورد نظر متوقف خواهد شد
- شناسه فایل حتماً باید به صورت JSON ارسال شود مثال: {"id: "file id}
