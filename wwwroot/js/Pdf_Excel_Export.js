function exportToPDF() {
    const doc = new window.jspdf.jsPDF({
        orientation: "landscape",
        unit: "mm",
        format: "a4"
    });

    // Move title closer to the top
    doc.setFont("helvetica", "bold");
    doc.setFontSize(16);
    doc.text("Sales Enquiry Report", 120, 10); // Reduced Y value for less top margin

    // Table headers with Serial Number (S.No)
    const headers = [["S.No", "Enquiry Date", "Company Name", "Referred By","Enquiry Details", "Customer Details", "Status", "Remainder Date", "Remainder Place"]];

    // Table body data with Serial Number
    const data = [];
    document.querySelectorAll(".datatable tbody tr").forEach((row, index) => {
        const rowData = [index + 1]; // Add Serial Number (starting from 1)
        row.querySelectorAll("td:not(:first-child, :last-child)").forEach(cell => {
            rowData.push(cell.textContent.trim());
        });
        data.push(rowData);
    });

    // Generate table in PDF with fixed column widths and reduced margins
    doc.autoTable({
        head: headers,
        body: data,
        startY: 15, // Reduced from 30 to 20 to move the table up
        theme: 'grid',
        margin: { left: 10, right: 10 }, // Reduce left and right padding
        styles: { fontSize: 9, cellPadding: 2 },
        headStyles: { fillColor: [40, 40, 40], textColor: [255, 255, 255] },
        columnStyles: {
            0: { cellWidth: 12 }, // Serial Number (S.No)
            1: { cellWidth: 20 }, // Enquiry Date
            2: { cellWidth: 30 }, // Referred By
            3: { cellWidth: 50 }, // Enquiry Details
            4: { cellWidth: 40 }, // Company Name
            5: { cellWidth: 50 }, // Customer Details
            6: { cellWidth: 25 }, // Status
            7: { cellWidth: 20 }, // Remainder Date
            8: { cellWidth: 30 }  // Remainder Place
        }
    });

    doc.save('Sales_Enquiries.pdf');
}

function exportToExcel() {
    let workbook = new ExcelJS.Workbook();
    let worksheet = workbook.addWorksheet("Sales Enquiries");
    debugger;
    // Table headers with Serial Number
    let headers = ["S.No", "Enquiry Date", "Company Name", "Referred By", "Enquiry Details", "Customer Details", "Status", "Remainder Date", "Remainder Place"];

    // Add headers to worksheet with styling
    let headerRow = worksheet.addRow(headers);
    headerRow.font = { bold: true, size: 14 }; // Bold + Increased font size
    headerRow.alignment = { horizontal: 'center' }; // Center align headers

    // Apply border and background color to headers
    headerRow.eachCell(cell => {
        cell.border = {
            top: { style: 'thin' },
            left: { style: 'thin' },
            bottom: { style: 'thin' },
            right: { style: 'thin' }
        };
        cell.fill = {
            type: 'pattern',
            pattern: 'solid',
            fgColor: { argb: 'FFFFCC00' } // Yellow background
        };
    });

    // Table body data with Serial Number
    let rowIndex = 1; // Start serial number from 1
    document.querySelectorAll(".datatable tbody tr").forEach(row => {
        let rowData = [rowIndex]; // Add Serial Number (S.No)

        row.querySelectorAll("td:not(:first-child, :last-child)").forEach(cell => {
            rowData.push(cell.textContent.trim());
        });

        let dataRow = worksheet.addRow(rowData);

        // Apply border to each data cell
        dataRow.eachCell(cell => {
            cell.border = {
                top: { style: 'thin' },
                left: { style: 'thin' },
                bottom: { style: 'thin' },
                right: { style: 'thin' }
            };
        });

        rowIndex++; // Increment serial number
    });

    // Auto-sizing columns
    worksheet.columns.forEach((column, index) => {
        if (index === 0) {
            column.width = 8; // Set a smaller width for S.No
            column.alignment = { horizontal: "center" };
        } else {
            column.width = 20; // Adjust column width for other columns
        }
    });

    // Create and download file
    workbook.xlsx.writeBuffer().then(buffer => {
        let blob = new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
        saveAs(blob, "Sales_Enquiries.xlsx");
    });
}


// Event Listeners for Buttons
document.getElementById('exportPdf').addEventListener('click', exportToPDF);
document.getElementById('exportExcel').addEventListener('click', exportToExcel);