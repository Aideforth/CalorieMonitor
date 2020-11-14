Create table Users(
Id bigint identity(1,1),
FirstName nvarchar(100) null, 
LastName nvarchar(100) null,
Role int null,
UserName nvarchar(50) null, 
Password nvarchar(100) null,
EmailAddress nvarchar(100) null,
DailyCalorieLimit float null,
DateCreated DateTime null,
DateUpdated DateTime null,
Primary Key (ID));

Create table MealEntries(
Id bigint identity(1,1),
Text nvarchar(250) null,
Calories float null, 
CaloriesStatus int null,
WithInDailyLimit bit null,
EntryUserId bigint null,
EntryCreatorId bigint null,
EntryDateTime DateTime null,
DateCreated DateTime null,
DateUpdated DateTime null,
Primary Key (ID));

Create table MealItems(
Id bigint identity(1,1),
Name nvarchar(100) null,
Calories float null, 
WeightInGrams float null,
CaloriePerGram float null,
MealEntryId bigint null,
DateCreated DateTime null,
DateUpdated DateTime null,
Primary Key (ID));

GO

Insert into Users (FirstName, LastName, Role, UserName, Password, EmailAddress, DailyCalorieLimit, DateCreated, DateUpdated) 
Values('First Name', 'Last Name', 2, 'Username', '??>??|9??O,oa.???[&??N9?#?8?"', 'email@mail.com', 1, GETDATE(), GETDATE());