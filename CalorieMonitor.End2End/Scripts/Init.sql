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

Insert into Users (Id, FirstName, LastName, Role, UserName, Password, EmailAddress, DailyCalorieLimit, DateCreated, DateUpdated) 
Values(1, 'Name', 'New', 2, 'user1', '??>??|9??O,oa.???[&??N9?#?8?"', 'email1@gmail.com', 100, date('now'), date('now')),
(2, 'Aide', 'New', 2, 'Aide4th', '??>??|9??O,oa.???[&??N9?#?8?"', 'email2@gmail.com', 100, date('now'), date('now')),
(3, 'Name', 'New', 0, 'user3', '??>??|9??O,oa.???[&??N9?#?8?"', 'email3@gmail.com', 100, date('now'), date('now')),
(4, 'Name', 'New', 1, 'user4', '??>??|9??O,oa.???[&??N9?#?8?"', 'email4@gmail.com', 100, date('now'), date('now')),
(5, 'Name', 'New', 0, 'user5', '??>??|9??O,oa.???[&??N9?#?8?"', 'email5@gmail.com', 100, date('now'), date('now')),
(6, 'Name', 'New', 0, 'user6', '??>??|9??O,oa.???[&??N9?#?8?"', 'email6@gmail.com', 100, date('now'), date('now')),
(7, 'Name', 'New', 0, 'user7', '??>??|9??O,oa.???[&??N9?#?8?"', 'email7@gmail.com', 100, date('now'), date('now')),
(8, 'Name', 'New', 2, 'user8', '??>??|9??O,oa.???[&??N9?#?8?"', 'email8@gmail.com', 100, date('now'), date('now')),
(9, 'Name', 'New', 0, 'user9', '??>??|9??O,oa.???[&??N9?#?8?"', 'email9@gmail.com', 100, date('now'), date('now')),
(10, 'Name', 'New', 1, 'user10', '??>??|9??O,oa.???[&??N9?#?8?"', 'email10@gmail.com', 100, date('now'), date('now'));

insert into MealEntries(Id, Text, Calories, CaloriesStatus, WithInDailyLimit, EntryUserId, EntryCreatorId, EntryDateTime, DateCreated, DateUpdated)
Values(1,'Text1', 100, 1, 1, 2, 2, '2020-10-20 00:00:00', date('now'), date('now')),
(2, 'Text2', 100, 1, 1, 2, 2, '2020-10-20 00:00:00', date('now'), date('now')),
(3, 'Text3', 100, 1, 1, 3, 3, '2020-10-20 00:00:00', date('now'), date('now')),
(4, 'Text4', 100, 1, 1, 3, 3, '2020-10-20 00:00:00', date('now'), date('now')),
(5, 'Text5', 100, 1, 1, 4, 4, '2020-10-20 00:00:00', date('now'), date('now')),
(6, 'Text6', 100, 1, 1, 4, 4, '2020-10-20 00:00:00', date('now'), date('now')),
(7, 'Text7', 100, 1, 1, 1, 1, '2020-10-20 00:00:00', date('now'), date('now')),
(8, 'Text8', 100, 1, 1, 1, 1, '2020-10-20 00:00:00', date('now'), date('now')),
(9, 'Text9', 100, 1, 1, 8, 8, '2020-10-20 00:00:00', date('now'), date('now')),
(10,'Text10', 100, 1, 1, 8, 8, '2020-10-20 00:00:00', date('now'), date('now'));