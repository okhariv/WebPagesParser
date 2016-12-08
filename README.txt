Web Pages Parser Application

Description
Application provides text checking on the web pages.

Input parameters
1. It was assumed that input text doesn’t contain html tags.
2. Url validation requires that input data link begins with http/https. 
For testing purposes link validation also allows local links, in http://localhost:1234/Home/Index – like format

Search process
1. Search by links is executed according to levels: 
- firstly handles urls form input page, 
- then all references from pages, that were obtained from the first step
- and so on.
Links processing order on the same level is non-deterministic.
2. Text search includes ordinal comparison (case-sensitive and culture-insensitive).

Testing
1. To test that 
- searching performs by levels 
- exceptions are thrown correctly
is offered to run WebSite application (simple web application with few number of connected pages)
2. To test with the bigger number of processed links or the performance – any valid links can be used.

Critical remarks
In the end of application writing, developer came to the conclusion that using ManualResetEvents objects for threads stopping was inappropriate decision. In situations when the number of links on the web page is greater than 64 an error may occur. 
