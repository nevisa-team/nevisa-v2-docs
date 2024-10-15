### راهنمای استفاده از سرویس نویسا

در این راهنما به شما آموزش داده می‌شود که چگونه از API های مختلف سرویس نویسا استفاده کنید. هر بخش شامل توضیحات مربوط به هر API و یک نمونه کد به زبان پایتون است.

#### ۱. دریافت لیست فایل‌ها (GET /api/files/get)

**شرح**: این API برای دریافت لیست فایل‌های آپلود شده به کار می‌رود.

**پارامترها**:
- `page` (اختیاری): شماره صفحه‌ای که می‌خواهید داده‌ها از آن بارگذاری شود.
- `sort` (اختیاری): مرتب‌سازی فایل‌ها بر اساس نام یا تاریخ. مثال: `{"field": "fileName", "sort": "asc"}`.
- `searchQuery` (اختیاری): جستجو در نام فایل‌ها.
- `status` (اختیاری): وضعیت فایل‌ها (مثل: `uploaded`, `pending`, `processing`, `succeeded`).

**نمونه کد پایتون**:
```python
import requests

url = "https://your-api.com/api/files/get"
params = {
    "page": 1,
    "sort": '{"field": "fileName", "sort": "asc"}',
    "searchQuery": "test",
    "status": "uploaded"
}
headers = {"Authorization": "Bearer YOUR_TOKEN"}

response = requests.get(url, params=params, headers=headers)

if response.status_code == 200:
    print(response.json())
else:
    print("Error:", response.status_code)
```

#### ۲. افزودن فایل (POST /api/files/add)

**شرح**: این API برای آپلود فایل به کار می‌رود.

**پارامترها**:
- `files` (ضروری): فایلی که باید آپلود شود.
- `usePunctuations` (ضروری): آیا از علامت‌گذاری استفاده شود یا خیر.
- `useTextToNumber` (ضروری): آیا متن به عدد تبدیل شود یا خیر.
- `userPlatform` (ضروری): پلتفرمی که از آن استفاده می‌کنید (مثل: `browser` یا `mobile`).
- `useDiarization` (ضروری):  آیا از Diarization (تشخیص گوینده) استفاده شود یا خیر..
**نمونه کد پایتون**:
```python
import requests

url = "https://your-api.com/api/files/add"
files = {'files': open('file.wav', 'rb')}
data = {
    "usePunctuations": True,
    "useTextToNumber": False,
    "useDiarization": True,  # استفاده از Diarization
    "userPlatform": "browser"
}
headers = {"Authorization": "Bearer YOUR_TOKEN"}

response = requests.post(url, files=files, data=data, headers=headers)

if response.status_code == 200:
    print("File uploaded successfully with diarization")
else:
    print("Error:", response.status_code)
```

#### ۳. تنظیم گزینه تبدیل متن به عدد (PUT /api/files/set-text-to-number)

**شرح**: این API برای تنظیم گزینه تبدیل متن به عدد برای فایل استفاده می‌شود.

**پارامترها**:
- `id` (ضروری): شناسه فایل.
- `useTextToNumber` (ضروری): وضعیت تبدیل متن به عدد (مثل: `true` یا `false`).

**نمونه کد پایتون**:
```python
import requests

url = "https://your-api.com/api/files/set-text-to-number"
data = {
    "id": "63da65bcc341e1571ee7dc93",
    "useTextToNumber": True
}
headers = {"Authorization": "Bearer YOUR_TOKEN"}

response = requests.put(url, json=data, headers=headers)

if response.status_code == 200:
    print("Text to number option set successfully")
else:
    print("Error:", response.status_code)
```

#### ۴. تنظیم گزینه استفاده از علائم نگارشی (PUT /api/files/set-punctuations)

**شرح**: این API برای تنظیم استفاده از علائم نگارشی در فایل استفاده می‌شود.

**پارامترها**:
- `id` (ضروری): شناسه فایل.
- `usePunctuations` (ضروری): وضعیت استفاده از علائم نگارشی (مثل: `true` یا `false`).

**نمونه کد پایتون**:
```python
import requests

url = "https://your-api.com/api/files/set-punctuations"
data = {
    "id": "63da65bcc341e1571ee7dc93",
    "usePunctuations": False
}
headers = {"Authorization": "Bearer YOUR_TOKEN"}

response = requests.put(url, json=data, headers=headers)

if response.status_code == 200:
    print("Punctuations option set successfully")
else:
    print("Error:", response.status_code)
```

#### ۵. حذف فایل (DELETE /api/files/remove)

**شرح**: این API برای حذف فایل استفاده می‌شود.

**پارامترها**:
- `id` (ضروری): شناسه فایل.

**نمونه کد پایتون**:
```python
import requests

url = "https://your-api.com/api/files/remove"
params = {
    "id": "63da65bcc341e1571ee7dc93"
}
headers = {"Authorization": "Bearer YOUR_TOKEN"}

response = requests.delete(url, params=params, headers=headers)

if response.status_code == 200:
    print("File deleted successfully")
else:
    print("Error:", response.status_code)
```

این راهنما شامل اطلاعات اصلی در مورد استفاده از API های نویسا بود. در صورت نیاز به اطلاعات بیشتر یا افزودن قابلیت‌های جدید، می‌توانید با پشتیبانی تماس بگیرید یا مستندات بیشتری مطالعه کنید.