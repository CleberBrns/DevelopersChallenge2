USE TransactionBankViewer
GO

CREATE TABLE BankAccount (

	Id bigint IDENTITY(1,1),
	AccountId bigint NOT NULL,
	BankId varchar(10) NOT NULL,
	
	CONSTRAINT PK_BankAccount PRIMARY KEY (Id)
);

