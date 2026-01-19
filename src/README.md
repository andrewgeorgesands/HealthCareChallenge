# Healthcare Senior Dev Challenge
                                                 
## Scenario
You have inherited a prototype for a patient monitoring dashboard. The previous developer vibe coded the solution then left abruptly. The code "works," but issues were found after it was pushed to production.

The production environment is https://vismaflythealthcarechallenge.azurewebsites.net/ if you want to start looking at it before getting it up and running locally.
                                                 
## Your Task
1. **Version control** Clone the code to your local machine
2. **Local development** Run the application locally 
3. **Authentication** Reach out to developer Andrew Sands to get creds for Visma Connect. Contact details are in the code.
4. **Authentication 2** Log in with Visma Connect.  You can use the "Create account" link to self-service a new account
5. **Race condition** Find and fix the race condition left behind by the previous developer
6. **Orphan record** Find and fix the hard delete bug that leaves orphaned records in the database.
7. **Idepotency** Find and fix the idempotency bug when retrieving patient vitals.
8. **Security** Find the rather big security whoopsie.
9. **Refactoring** Refactor the Vitals index action method to separate the logic for fetching vitals from the logic for rendering the view
10. **Test** Add a unit test that shows the race condition is fixed by your changes

# Artifacts
1. Make your code changes as a pull request so we can review it


---


# Assignment
### Assignment: Race condition
There is a thread 1 & 2 issue, where if one thread updates the inventory 
while another thread is also trying to update it, both could succeed and 
cause the inventory to go negative.

There are a few ways it could be solved:
1. do a RAW SQL(not something i like and makes it more unmaintanable as it also bypasses EF)
2. Add a scoped transaction(might be good for easier to understand, but alone it doesnt fix it, needs to combine with serializable isolation level which adds peformance costs )
3. Least viable is row history/row version, for obv reasons not a good solution. 

But the easiest fix that is quite simple is to add `[ConcurrencyCheck]` to the model then catch
the exception. Its also easy to test if it works or not and least maintainence. 

**Explanation**:
`[Concurrency check]` in `QuantityInStock` tells ef core to include the original value in the `WHERE` clause on update. 
If another thread already changed QuantityInStock, the WHERE clause won't match, EF Core throws DbUpdateConcurrencyException, and the controller shows an error 
message instead of dispensing twice. Then it is handled in frontend by the alert banner. 


### Assignment Orphaned records

The patient delete doesnt have cascade behavior, as it resitricts it. inorder to avoid orphaned records, we would store reference then delete the vitals after removing patient.

### Assignment:Idempotency
The Vitals is a get REST principle problem. Instead of doing a save changes, i changed it to actually be a GET principle that only returns current values and doesnt update. 
No tests needed. 

### Assignment: Security whoopsie
1. First is the missing [Authorize] attribute on patientcontroller. instead of doing a authorize on each method, we do rather a authorize on the whole controller so 
that it is easier to maintain. since one can assume that a patient is always a authorized information

2. is that there is a logging of SSN of patient. This should not happen and is it necessary as it might be returned to external intergrations.


### Assignment: Refactoring the vitals logic
Refactored patient into a own service. This adds a service layer but it is much better since it promotes these:
- Separation of concerns - controller handles HTTP, service handles data
- Makes unit testing easier - can mock IPatientService
- Single Responsibility Principle
- Service can be reused by other controllers or APIs

 The same pattern could be applied to other actions, which i did. I would argument that would be easier to read to leave it alone, but i had alot of time so 
 i decided to do it anyway. 

- 
 Other:
 - Removed developer page in development. 
 - Added clearer attributes to patient controller instead of comments
 - refactored patient service and patient controller