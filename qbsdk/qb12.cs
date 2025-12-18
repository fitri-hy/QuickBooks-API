using System;
using System.Collections;
using System.Text;
using System.Web.Script.Serialization;
using Interop.QBFC12;

class QB12App
{
    static void Main(string[] args)
    {
        QBSessionManager sess = null;

        try
        {
            sess = new QBSessionManager();
            sess.OpenConnection("", "QB SDK 12 Connection Only");
            sess.BeginSession("", ENOpenMode.omDontCare);

            IMsgSetRequest requestMsgSet = sess.CreateMsgSetRequest("US", 8, 0);
            requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;

            ArrayList invoiceDataList = new ArrayList();
            string filterCustomer = args.Length >= 1 ? args[0] : null;

            // ===============================
            // 1. Query all customers
            // ===============================
            ICustomerQuery customerQuery = requestMsgSet.AppendCustomerQueryRq();
            customerQuery.IncludeRetElementList.Add("ListID");
            customerQuery.IncludeRetElementList.Add("FullName");
            customerQuery.IncludeRetElementList.Add("CompanyName");
            customerQuery.IncludeRetElementList.Add("Phone");
            customerQuery.IncludeRetElementList.Add("Email");
            customerQuery.IncludeRetElementList.Add("BillAddress");
            customerQuery.IncludeRetElementList.Add("ShipAddress");

            // ===============================
            // 2. Query all invoices
            // ===============================
            IInvoiceQuery invoiceQuery = requestMsgSet.AppendInvoiceQueryRq();
            invoiceQuery.IncludeLineItems.SetValue(true);

            IMsgSetResponse responseMsgSet = sess.DoRequests(requestMsgSet);

            // ===============================
            // Build customer lookup
            // ===============================
            Hashtable customerMap = new Hashtable();
            IResponse customerResponse = responseMsgSet.ResponseList.GetAt(0);
            if (customerResponse.Detail != null)
            {
                ICustomerRetList custList = customerResponse.Detail as ICustomerRetList;
                if (custList != null)
                {
                    for (int i = 0; i < custList.Count; i++)
                    {
                        ICustomerRet cust = custList.GetAt(i);
                        string custName = cust.FullName != null ? cust.FullName.GetValue() : "";
                        customerMap[custName] = cust;
                    }
                }
            }

            // ===============================
            // Parse invoices
            // ===============================
            IResponse invoiceResponse = responseMsgSet.ResponseList.GetAt(1);
            if (invoiceResponse.Detail != null)
            {
                IInvoiceRetList list = invoiceResponse.Detail as IInvoiceRetList;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        IInvoiceRet txn = list.GetAt(i);
                        if (txn != null)
                        {
                            string customerName = (txn.CustomerRef != null && txn.CustomerRef.FullName != null)
                                ? txn.CustomerRef.FullName.GetValue()
                                : "";

                            if (!string.IsNullOrEmpty(filterCustomer) && !customerName.Contains(filterCustomer))
                                continue;

                            Hashtable txnData = new Hashtable();

                            // ===============================
                            // Customer detail (di atas)
                            // ===============================
                            Hashtable customerData = new Hashtable();
                            if (customerMap.ContainsKey(customerName))
                            {
                                ICustomerRet cust = (ICustomerRet)customerMap[customerName];
                                customerData["ListID"] = cust.ListID != null ? cust.ListID.GetValue() : "";
                                customerData["FullName"] = cust.FullName != null ? cust.FullName.GetValue() : "";
                                customerData["CompanyName"] = cust.CompanyName != null ? cust.CompanyName.GetValue() : "";
                                customerData["Phone"] = cust.Phone != null ? cust.Phone.GetValue() : "";
                                customerData["Email"] = cust.Email != null ? cust.Email.GetValue() : "";
                                customerData["BillAddress"] = cust.BillAddress != null ? cust.BillAddress.Addr1.GetValue() : "";
                                customerData["ShipAddress"] = cust.ShipAddress != null ? cust.ShipAddress.Addr1.GetValue() : "";
                            }
                            txnData["Customer"] = customerData;

                            // ===============================
                            // Invoice header
                            // ===============================
                            txnData["TxnID"] = txn.TxnID != null ? txn.TxnID.GetValue() : "";
                            txnData["TxnNumber"] = txn.TxnNumber != null ? txn.TxnNumber.GetValue() : 0;
                            txnData["TxnDate"] = txn.TxnDate != null ? txn.TxnDate.GetValue().ToString("yyyy-MM-dd") : "";
                            txnData["SalesRep"] = (txn.SalesRepRef != null && txn.SalesRepRef.FullName != null) ? txn.SalesRepRef.FullName.GetValue() : "";
                            txnData["Terms"] = (txn.TermsRef != null && txn.TermsRef.FullName != null) ? txn.TermsRef.FullName.GetValue() : "";
                            txnData["Memo"] = txn.Memo != null ? txn.Memo.GetValue() : "";
                            txnData["BillAddress"] = txn.BillAddress != null ? txn.BillAddress.Addr1.GetValue() : "";
                            txnData["ShipAddress"] = txn.ShipAddress != null ? txn.ShipAddress.Addr1.GetValue() : "";
                            txnData["Subtotal"] = txn.Subtotal != null ? txn.Subtotal.GetValue() : 0;
                            txnData["SalesTaxTotal"] = txn.SalesTaxTotal != null ? txn.SalesTaxTotal.GetValue() : 0;
                            txnData["BalanceRemaining"] = txn.BalanceRemaining != null ? txn.BalanceRemaining.GetValue() : 0;

                            // ===============================
                            // Line Items
                            // ===============================
                            ArrayList items = new ArrayList();
                            if (txn.ORInvoiceLineRetList != null)
                            {
                                for (int j = 0; j < txn.ORInvoiceLineRetList.Count; j++)
                                {
                                    IORInvoiceLineRet orLine = txn.ORInvoiceLineRetList.GetAt(j);
                                    if (orLine.InvoiceLineRet != null)
                                    {
                                        IInvoiceLineRet line = orLine.InvoiceLineRet;
                                        Hashtable lineData = new Hashtable();

                                        lineData["ItemRef"] = (line.ItemRef != null && line.ItemRef.FullName != null) ? line.ItemRef.FullName.GetValue() : "";
                                        lineData["Desc"] = line.Desc != null ? line.Desc.GetValue() : "";
                                        lineData["Quantity"] = line.Quantity != null ? line.Quantity.GetValue() : 0;
                                        lineData["Amount"] = line.Amount != null ? line.Amount.GetValue() : 0;

                                        items.Add(lineData);
                                    }
                                }
                            }
                            txnData["LineItems"] = items;

                            invoiceDataList.Add(txnData);
                        }
                    }
                }
            }

            // Serialize JSON
            JavaScriptSerializer js = new JavaScriptSerializer();
            string json = js.Serialize(invoiceDataList);
            Console.WriteLine(PrettyPrintJson(json));
        }
        catch (Exception ex)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string errorJson = js.Serialize(new Hashtable { { "error", ex.Message } });
            Console.WriteLine(PrettyPrintJson(errorJson));
        }
        finally
        {
            if (sess != null)
            {
                try
                {
                    sess.EndSession();
                    sess.CloseConnection();
                }
                catch { }
            }
        }
    }

    static string PrettyPrintJson(string json)
    {
        int indent = 0;
        bool inQuotes = false;
        var sb = new StringBuilder();

        foreach (char ch in json)
        {
            switch (ch)
            {
                case '{':
                case '[':
                    sb.Append(ch);
                    if (!inQuotes)
                    {
                        sb.AppendLine();
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                    }
                    break;

                case '}':
                case ']':
                    if (!inQuotes)
                    {
                        sb.AppendLine();
                        indent--;
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(ch);
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                    break;

                case ',':
                    sb.Append(ch);
                    if (!inQuotes)
                    {
                        sb.AppendLine();
                        sb.Append(new string(' ', indent * 2));
                    }
                    break;

                case '"':
                    sb.Append(ch);
                    if (sb.Length > 1 && sb[sb.Length - 2] != '\\')
                        inQuotes = !inQuotes;
                    break;

                default:
                    sb.Append(ch);
                    break;
            }
        }

        return sb.ToString();
    }
}
