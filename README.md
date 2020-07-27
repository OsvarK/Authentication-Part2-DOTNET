# Authentication using React & .NET
This is the backend part, View the React frontend part [here!](https://github.com/OsvarK/AuthProject-Part1-React)
# Introduction 
The goal of this project is to learn the .NET framework, but also get some understanding on how i can handle authentication using tokens. I'm using React as a frontend framework because im quite familiar with it. Because of my first time creating something like this, there is probely some mistakes or bad practises that i'm unaware off and should therefore never be used as a real application.  

# API Calls
| Method | Routes | Description | Requires token |
| ------ | ------ | ------ | ------ |
| POST | /api/auth/login | Creates a token in the browsers to login the user | False, (but requires login credentials) |
| POST | /api/auth/signup | Creates a new user | False |
| POST | /api/auth/edituser | Edits the logged in users information | True |
| POST | /api/auth/authgoogle | Logins or creates an account using google (requires google's TokenID) | False |
| GET | /api/auth/auth | Returns the users information | True |
| GET | /api/auth/logout | Terminates the token in the browsers to logout the user | True |
| GET | /api/auth/isadmin | Return the users admin status | True |
| GET | /api/auth/isadmin | Return the users admin status | True |


### Tech
* [MySQL] - MySQL for the database
* [JWT] - For the token authentication

### Database Table Example 

| UserID | Username | Firstname | Lastname | Email | Password | GoogleSubjectID | Admin |
| ------ | ------ | ------ | ------ | ------ | ------ | ------ |
| 0 | John69 | John | Doe | John.Doe69@Email.com | Dl2jJFVqNTUoJFkrdiFFZey2dsO5yOtd6ytVSyd3L3/Nz7hh | NULL | 0 |
| 1 | OscarAdmin123 | Oscar | Karlsson | Oscar.karlsson@Email.com | Dl2jJFVqN3UoJhkrdiFxZey2ds45yztd6xtVSyd3L3/Nd5hh | NULL | 1 |

### Is the password safe

The passwords are salted & hashed. using this function. 
```csharp
        public static string HashPassword(string unHashedPassword)
        {
            // Salt
            byte[] salt = Encoding.ASCII.GetBytes(ConfigContex.GetSalt());

            // Hash password
            var pbkdf2 = new Rfc2898DeriveBytes(unHashedPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Combine salt and Password
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Stringyfy password and return it
            string hasedPassword = Convert.ToBase64String(hashBytes);
            return hasedPassword;
        }
```
To determien if two passwords are the same password we just tests if the both hased version of the passwords are the same.

## What is the Action Class?
I don’t like when data is beeing passed from the controller directly to the queries, and there for i have a class between them there i can process the data before it goes into queries (if needed). As business logic i guess.
## Why not stores procedures?
i'm not comfortable workign with them yet, becouse i havent done enough reading.