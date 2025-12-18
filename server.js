const express = require("express");
const { execFile } = require("child_process");
const path = require("path");
const app = express();

const QB_EXE = path.join(__dirname, "qbsdk/qb12.exe");

app.get("/api/invoices", (req, res) => {
    const { from, to, customer, page, limit } = req.query;

    const pageNum = parseInt(page) || 1;
    const pageSize = parseInt(limit) || 50;

    execFile(QB_EXE, [customer || ""], (err, stdout, stderr) => {
        if (err) return res.status(500).json({ error: err.message });

        try {
            let data = JSON.parse(stdout);

            if (from || to) {
                const fromDate = from ? new Date(from) : null;
                const toDate = to ? new Date(to) : null;

                data = data.filter(inv => {
                    const txnDate = new Date(inv.TxnDate);
                    if (fromDate && txnDate < fromDate) return false;
                    if (toDate && txnDate > toDate) return false;
                    return true;
                });
            }

            const totalItems = data.length;
            const totalPages = Math.ceil(totalItems / pageSize);
            const start = (pageNum - 1) * pageSize;
            const pagedData = data.slice(start, start + pageSize);

            res.json({
                page: pageNum,
                limit: pageSize,
                totalItems,
                totalPages,
                data: pagedData
            });
        } catch (e) {
            res.status(500).json({ error: "Failed to parse output", raw: stdout });
        }
    });
});

app.listen(3000, () => console.log("API running at http://localhost:3000"));
