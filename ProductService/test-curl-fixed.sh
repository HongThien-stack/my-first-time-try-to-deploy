# ==================================================
# CURL COMMAND ĐÃ SỬA - Dùng CategoryId thật từ database
# ==================================================

# Copy và chạy command này (thay đổi path đến file ảnh phù hợp)

curl -X 'POST' \
  'http://localhost:5001/api/Product' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyMjIyZDc2ZS00NDc4LTQ2ZjgtOGYxNC05YWZmZjg3YzI5MjEiLCJlbWFpbCI6ImFkbWluQGNvbXBhbnkuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJyb2xlX2lkIjoiMSIsInN0YXR1cyI6IkFDVElWRSIsImp0aSI6IjBjMjAxZDE5LTI3NDUtNDhmZC05MmNiLWY4MWY1MGM1MDk0NyIsImV4cCI6MTc3MjEzMDUwOCwiaXNzIjoiSWRlbnRpdHlTZXJ2aWNlIiwiYXVkIjoiSWRlbnRpdHlTZXJ2aWNlQ2xpZW50In0.wPKvxDVBS0q9iRDZxZRahdcf8rbg5GM2nyCa30eO7pY' \
  -H 'Content-Type: multipart/form-data' \
  -F 'Sku=TEST-20260227-001' \
  -F 'Name=Nước Cam Việt Quất' \
  -F 'CategoryId=9639C3EB-A1C0-4100-B65E-0B3ADD9FA64C' \
  -F 'Price=50000' \
  -F 'Unit=Thùng' \
  -F 'Brand=Việt Nam Juice' \
  -F 'Origin=Việt Nam' \
  -F 'Description=Nước ép cam tươi nguyên chất 100%' \
  -F 'IsPerishable=true' \
  -F 'ShelfLifeDays=180' \
  -F 'IsAvailable=true' \
  -F 'IsNew=true' \
  -F 'IsFeatured=true' \
  -F 'IsOnSale=true' \
  -F 'OriginalPrice=60000' \
  -F 'CostPrice=20000' \
  -F 'MinOrderQuantity=1' \
  -F 'MaxOrderQuantity=100' \
  -F 'QuantityPerUnit=24' \
  -F 'Barcode=8123456789012' \
  -F 'StorageInstructions=Bảo quản nơi khô ráo, thoáng mát' \
  -F 'MainImage=@9dd15e6c087521bdc25b3c004cec377d.jpg;type=image/jpeg' \
  -F 'AdditionalImages=@9dd15e6c087521bdc25b3c004cec377d.jpg;type=image/jpeg'

# ==================================================
# Hoặc test với Swagger UI:
# ==================================================
# 1. Mở: http://localhost:5001/swagger
# 2. Authorize với token
# 3. Dùng CategoryId: 9639C3EB-A1C0-4100-B65E-0B3ADD9FA64C
