using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLayer.Services;

namespace UnitTesting
{
    [TestClass]
    public class EmailServiceUT
    {
        EmailService es;
        public EmailServiceUT()
        {
            es = new EmailService();
        }

        [TestMethod]
        public void createMessage_Pass()
        {
            //Arrange
            string ExpectedReceiverName = "Bob";
            string ExpectedReceiverEmail = "Bob@website.com";
            string ExpectedEmailSubject = "Message Test";
            string ExpectedEmailBody = "This is a test body";

            string ActualReceiverName = "Bob";
            string ActualReceiverEmail = "Bob@website.com";
            string ActualEmailSubject = "Message Test";
            string ActualEmailBody = "This is a test body";
            //Act
            var expected = es.createEmailPlainBody(ExpectedReceiverName, ExpectedReceiverEmail, ExpectedEmailSubject, ExpectedEmailBody);
            var actual = es.createEmailPlainBody(ActualReceiverName, ActualReceiverEmail, ActualEmailSubject, ActualEmailBody);
            //Assert
            Assert.AreEqual(actual.To.ToString(), expected.To.ToString());
            Assert.AreEqual(actual.From.ToString(), expected.From.ToString());
            Assert.AreEqual(actual.Subject, expected.Subject);
            Assert.AreEqual(actual.TextBody, expected.TextBody);
        }

        [TestMethod]
        public void createMessage_Fail()
        {
            //Arrange
            string ExpectedReceiverName = "Bob";
            string ExpectedReceiverEmail = "Bob@website.com";
            string ExpectedEmailSubject = "Message Test";
            string ExpectedEmailBody = "This is a test body";

            string ActualReceiverName = "Alice";
            string ActualReceiverEmail = "Alice@website.com";
            string ActualEmailSubject = "Message Testz";
            string ActualEmailBody = "This is a test bodyz";
            //Act
            var expected = es.createEmailPlainBody(ExpectedReceiverName, ExpectedReceiverEmail, ExpectedEmailSubject, ExpectedEmailBody);
            var actual = es.createEmailPlainBody(ActualReceiverName, ActualReceiverEmail, ActualEmailSubject, ActualEmailBody);
            //Assert
            Assert.AreNotEqual(actual.To, expected.To);
            Assert.AreEqual(actual.From.ToString(), expected.From.ToString());
            Assert.AreNotEqual(actual.Subject, expected.Subject);
            Assert.AreNotEqual(actual.TextBody, expected.TextBody);
        }
    }
}
