USE TransactionBankViewer
GO

CREATE TABLE AccountTransaction (

	Id bigint IDENTITY(1,1),
	IdBankAccount bigint NOT NULL,
	TrType varchar(20) NOT NULL,
	DatePosted datetime NOT	NULL,
	Amount decimal(18,2) NOT NULL, 
	TrDescription varchar(100) NULL,
	DateImportation datetime NOT NULL,

	CONSTRAINT PK_AccountTransaction PRIMARY KEY (Id)
);



ALTER TABLE AccountTransaction  WITH CHECK ADD  CONSTRAINT [FK_AccountTransaction_BankAccount] FOREIGN KEY([IdBankAccount])
REFERENCES BankAccount ([Id])
GO

ALTER TABLE AccountTransaction CHECK CONSTRAINT [FK_AccountTransaction_BankAccount]
GO