SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.SurveyQuestion', N'U') IS NULL
        THROW 50001, 'Required table dbo.SurveyQuestion does not exist.', 1;

    IF OBJECT_ID(N'dbo.SurveyFormAnswer', N'U') IS NULL
        THROW 50002, 'Required table dbo.SurveyFormAnswer does not exist.', 1;

    IF COL_LENGTH(N'dbo.SurveyQuestion', N'IsDeleted') IS NULL
    BEGIN
        ALTER TABLE dbo.SurveyQuestion
            ADD IsDeleted bit NOT NULL
                CONSTRAINT DF_SurveyQuestion_IsDeleted DEFAULT (0) WITH VALUES;
    END;

    IF COL_LENGTH(N'dbo.SurveyFormAnswer', N'QuestionTextSnapshot') IS NULL
        ALTER TABLE dbo.SurveyFormAnswer ADD QuestionTextSnapshot nvarchar(1000) NULL;

    IF COL_LENGTH(N'dbo.SurveyFormAnswer', N'QuestionTypeSnapshot') IS NULL
        ALTER TABLE dbo.SurveyFormAnswer ADD QuestionTypeSnapshot nvarchar(50) NULL;

    IF COL_LENGTH(N'dbo.SurveyFormAnswer', N'QuestionDisplayOrderSnapshot') IS NULL
        ALTER TABLE dbo.SurveyFormAnswer ADD QuestionDisplayOrderSnapshot int NULL;

    EXEC sp_executesql N'
        UPDATE answer
        SET QuestionTextSnapshot = COALESCE(answer.QuestionTextSnapshot, question.QuestionText),
            QuestionTypeSnapshot = COALESCE(answer.QuestionTypeSnapshot, question.QuestionType),
            QuestionDisplayOrderSnapshot = COALESCE(answer.QuestionDisplayOrderSnapshot, question.DisplayOrder)
        FROM dbo.SurveyFormAnswer AS answer
        INNER JOIN dbo.SurveyQuestion AS question
            ON question.Id = answer.SurveyQuestionId
        WHERE answer.QuestionTextSnapshot IS NULL
           OR answer.QuestionTypeSnapshot IS NULL
           OR answer.QuestionDisplayOrderSnapshot IS NULL;';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
