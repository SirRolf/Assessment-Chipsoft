Assessment Chipsoft
===================================

## Description
For the Assessment of Chipsoft I was requested to create a ASP.NET Core REST API that can help two parties Post and Get information about a patient.
It is requested that i don't spend longer then 4 hours on this project

## Required Features
- Able to share add and adjust information about a patient
- Able to share documents through HTTP Requests
- Correct Error handling
- Create logs when information is exchanged
- Optionally: Use of a database is permitted however In-Memory data is also allowed.

## Improvement points
- I would like to have seperate put routes instead of reusing the Post routes for both the PatientInfo and the upload
- For such a small project with this time frame i should have kept it simple and not have added the backlog of patient info
- Implement correct anti Forgery
- error handling for CopyTo as it could return issues at /PatientDatabase/upload
- Use of Version for versions of patient info
- Use of Logger instead of Stringbuilder

## Design choises
### Use of minimal API
Due to the time constraint i decided to use a Minimal API to reduce setup time.

### Dictionary with list of patients
instead of having just one PatientInfo i create a backlog for every patient. The idea is that if someone sends data that is incorrect we would stil have a backlog we can go back to to revert changes if nececary.

### Save to LocalApplicationData
Since everything is being done localy on memory i decided to save the log files to the LocalApplicationData. Since i already had a directory path setup i decided  to use the same one for the documents.
