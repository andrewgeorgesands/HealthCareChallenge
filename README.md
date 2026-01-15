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
