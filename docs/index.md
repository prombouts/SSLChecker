## Welcome to GitHub Pages for the SSL Checker

This really simple project contains one Azure Function to check SSL certificates of public URLs on expiry.

## Usage

- Open the Visual Studio Solution (.sln) file and run the project
- Open Postman and do a GET request: http://localhost:7071/api/CheckSSLExpiry?domain=github.com&daysbeforeexpiry=14

Play with the domain and daysbeforeexpiry to see when you get an alert!

If you omit daysbeforeexpiry it will take the default of 10 days

## Parameters

| Name  | Value |
|-------|--------|
| domain  | Valid domain name like github.com |
| daysbeforeexpiry | Integer (10 by default) |

## Result values
If for example your SSL certificate expires in 100 days, and you call the function with 90 days, the result will be 200 with text 'No problems found'

If you call the function and the SSL certificate expires within the amount of days, you will get statuscode 500 and text 'Warning, SSL certificate is expired or expires within n days', where n is the integer you provided.