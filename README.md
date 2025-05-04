Tickets Data Aggregator
This console application automates the process of reading ticket information from PDF files—each containing a movie title, website URL, date, and time—then normalizes and aggregates those entries into a single text file (aggregatedTickets.txt). It supports culture-specific date parsing based on the ticket’s website domain (for example, .fr uses French date formats), converts all times to UTC, and reports any individual-file parsing errors without halting the entire run.

Features
Recursive PDF discovery
Scans a specified folder for all *.pdf files.

Structured extraction
Expects each ticket to occupy four lines:

Movie Title: <title>

Website: <url>

Date: <date>

Time: <time>

Culture-aware parsing
Detects date format by checking the ticket’s URL domain:

.com → en-US

.fr → fr-FR

.jp → ja-JP
Falls back to invariant culture if unrecognized.

UTC normalization
Converts local show times into Coordinated Universal Time before aggregation.

Robust error handling
Logs per-file warnings for malformed tickets and exits gracefully if no tickets are found.

Single output file
Writes aggregatedTickets.txt in the source folder, overwriting any existing file.

Warning: This code has not been tested end-to-end. Please add unit and integration tests before relying on it in production.

Links
https://github.com/UglyToad/PdfPig – the PdfPig library used to read text from PDF documents

Acknowledgement
This project was completed as part of the Udemy course Complete C# Masterclass by Krystyna Ślusarczyk (https://www.udemy.com/course/ultimate-csharp-masterclass/). Having significant prior experience with C#, my goal was to refresh my skills and familiarize myself with the latest updates, features, and best practices introduced in recent versions.
