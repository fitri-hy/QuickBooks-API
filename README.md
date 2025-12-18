# QuickBooks API (QBFC12)

> Integration with **Intuit QuickBooks Desktop Enterprise Solutions - Accountant Edition 12.0**

This project provides an API to **access and display data from QuickBooks Desktop Enterprise 12.0** using the **QBFC12 SDK**.

## Requirements

Before getting started, make sure you have the following prerequisites:

1. **QuickBooks Desktop Enterprise 12.0** installed on your system.
2. **QBFC12 SDK** installed. You can run it from the folder `qbsdk/qbsdk120.exe`.

## SDK Installation

Run `qbsdk120.exe` and follow the installation instructions. Make sure the SDK is registered and accessible from your project.

## Compile C# Application (QB12App)

Use the **Command Prompt** to compile `qb12.cs` into the executable `qb12.exe`.

> Adjust the **path** according to your file locations.

```cmd
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" /platform:x86 /reference:"C:\Program Files (x86)\Common Files\Intuit\QuickBooks\Interop.QBFC12.dll" "C:\YOUR_PROJECT\quickbook-api\qbsdk\qb12.cs"
```

> **Note:** Ensure **QuickBooks Desktop** is open before running the application, as the SDK requires an active QuickBooks session.

## Setup Node.js API

**Install dependencies:**

```bash
npm install
```

**Start the server:**

```bash
npm start
```

The server runs at: `http://localhost:3000`

## API Endpoints (Invoices)

| Endpoint        | Method | Query Parameters                                     | Description                                                                             |
| --------------- | ------ | ---------------------------------------------------- | --------------------------------------------------------------------------------------- |
| `/api/invoices` | GET    | `customer`, `from`, `to`, `page`, `limit` (optional) | Retrieves a list of invoices with optional filters for customer, dates, and pagination. |


### Query Parameters

| Parameter  | Type                | Required | Default | Description               |
| ---------- | ------------------- | -------- | ------- | ------------------------- |
| `customer` | string              | Optional | -       | Filter by customer name.  |
| `from`     | string (yyyy-MM-dd) | Optional | -       | Start date for filtering. |
| `to`       | string (yyyy-MM-dd) | Optional | -       | End date for filtering.   |
| `page`     | integer             | Optional | 1       | Pagination page number.   |
| `limit`    | integer             | Optional | 50      | Number of items per page. |

### Example Requests

```http
GET /api/invoices
GET /api/invoices?customer=John
GET /api/invoices?from=2025-12-01&to=2025-12-31
GET /api/invoices?customer=John&from=2025-12-01&to=2025-12-31
GET /api/invoices?page=1&limit=50
GET /api/invoices?customer=John&from=2025-12-01&to=2025-12-31&page=2&limit=100
```

### Example JSON Response

```json
{
  "page": 1,
  "limit": 50,
  "totalItems": 2,
  "totalPages": 1,
  "data": [
    {
      "BillAddress": "12 Sparrow St, Jakarta",
      "ShipAddress": "45 Orchid St, Jakarta",
      "BalanceRemaining": 1200,
      "TxnNumber": 101,
      "SalesTaxTotal": 120,
      "Subtotal": 1080,
      "Terms": "Net 30",
      "TxnDate": "2025-12-18",
      "LineItems": [
        {
          "Amount": 1080,
          "ItemRef": "Dell XPS 15 Laptop",
          "Quantity": 2,
          "Desc": "Laptop for office use"
        }
      ],
      "SalesRep": "Andy Wijaya",
      "TxnID": "1-987654321",
      "Customer": {
        "Phone": "+6281234567890",
        "ListID": "80000002-987654321",
        "CompanyName": "Maju Jaya Ltd.",
        "ShipAddress": "45 Orchid St, Jakarta",
        "FullName": "Budi Santoso",
        "BillAddress": "12 Sparrow St, Jakarta",
        "Email": "budi.santoso@majujaya.com"
      },
      "Memo": "Payment via bank transfer"
    }
  ]
}
```

## Contributing

We welcome contributions!

1. **Fork** the repository and create a branch.
2. **Make changes** and ensure the code works.
3. **Commit** with a clear message.
4. **Push** your branch and open a **Pull Request**.

Keep code style consistent and document new features. By contributing, you agree your changes are under the same project license.