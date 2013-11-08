namespace NBug.Tests.Unit.Helpers
{
  using System.Net;
  using System.Net.Mail;

  using NBug.Helpers;

  using Xunit;

  public class EmailDestinationBuilderTests
  {
    private const string ServerName = "smtp.test.com";

    [Fact]
    public void BuildAttachmentsAndServerOptionsWithPort_PreparesCorrectDestination()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);
      var credential = new NetworkCredential("test", "foo");

      string result = builder.SendAttachments().UseServer(true, 99, credential).Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};To={1},{2};UseAttachment={3};SmtpServer={4};UseSSL=yes;Port=99;UseAuthentication=yes;Username={5};Password={6};", 
          fromAddress.Address, 
          toAddress.Address, 
          anotherAddress.Address, 
          "true", 
          ServerName, 
          "test", 
          "foo");

      Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildAttachmentsAndShortServerOptions_PreparesCorrectDestination()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);
      var credential = new NetworkCredential("test", "foo");

      string result = builder.SendAttachments().UseServer(false, credential).Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};To={1},{2};UseAttachment={3};SmtpServer={4};UseSSL=no;Port=25;UseAuthentication=yes;Username={5};Password={6};", 
          fromAddress.Address, 
          toAddress.Address, 
          anotherAddress.Address, 
          "true", 
          ServerName, 
          "test", 
          "foo");

      Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildCustomBody_PreparesCorrectDestination()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);
      string result = builder.Body("Bizz").Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};To={1},{2};UseAttachment={3};CustomBody={4};SmtpServer={5};UseSSL=yes;Port=465;UseAuthentication=no;", 
          fromAddress.Address, 
          toAddress.Address, 
          anotherAddress.Address, 
          "false", 
          "Bizz", 
          ServerName);

      Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildCustomSubject_PreparesCorrectDestination()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);
      string result = builder.Subject("Buzz").Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};To={1},{2};UseAttachment={3};CustomSubject={4};SmtpServer={5};UseSSL=yes;Port=465;UseAuthentication=no;", 
          fromAddress.Address, 
          toAddress.Address, 
          anotherAddress.Address, 
          "false", 
          "Buzz", 
          ServerName);

      Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildFromName_PreparesCorrectDestination()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);
      string result = builder.FromName("Homer").Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};FromName={1};To={2},{3};UseAttachment={4};SmtpServer={5};UseSSL=yes;Port=465;UseAuthentication=no;", 
          fromAddress.Address, 
          "Homer", 
          toAddress.Address, 
          anotherAddress.Address, 
          "false", 
          ServerName);

      Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildPriority_PreparesCorrectDestination()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);
      string result = builder.Priority(MailPriority.High).Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};To={1},{2};UseAttachment={3};SmtpServer={4};UseSSL=yes;Port=465;Priority={5};UseAuthentication=no;", 
          fromAddress.Address, 
          toAddress.Address, 
          anotherAddress.Address, 
          "false", 
          ServerName, 
          MailPriority.High);

      Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildReplyTo_PreparesCorrectDestination()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);
      string result = builder.ReplyTo("B@rt").Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};To={1},{2};ReplyTo={3};UseAttachment={4};SmtpServer={5};UseSSL=yes;Port=465;UseAuthentication=no;", 
          fromAddress.Address, 
          toAddress.Address, 
          anotherAddress.Address, 
          "B@rt", 
          "false", 
          ServerName);

      Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildWithBcc_DefaultsToSecuredAnonymousConnectionNoAttachments()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");

      var bcc1 = new MailAddress("bazz@test.com");
      var bcc2 = new MailAddress("fuzz@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);

      string result = builder.Bcc(new[] { bcc1, bcc2 }).Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};To={1},{2};Bcc={3},{4};UseAttachment={5};SmtpServer={6};UseSSL=yes;Port=465;UseAuthentication=no;", 
          fromAddress.Address, 
          toAddress.Address, 
          anotherAddress.Address, 
          bcc1.Address, 
          bcc2.Address, 
          "false", 
          ServerName);

      Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildWithCc_DefaultsToSecuredAnonymousConnectionNoAttachments()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");
      var cc1 = new MailAddress("bazz@test.com");
      var cc2 = new MailAddress("fuzz@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);

      string result = builder.Cc(new[] { cc1, cc2 }).Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};To={1},{2};Cc={3},{4};UseAttachment={5};SmtpServer={6};UseSSL=yes;Port=465;UseAuthentication=no;", 
          fromAddress.Address, 
          toAddress.Address, 
          anotherAddress.Address, 
          cc1.Address, 
          cc2.Address, 
          "false", 
          ServerName);

      Assert.Equal(expected, result);
    }

    [Fact]
    public void Build_DefaultsToSecuredAnonymousConnectionNoAttachments()
    {
      // Arrange
      var fromAddress = new MailAddress("bar@test.com");
      var toAddress = new MailAddress("foo@test.com");
      var anotherAddress = new MailAddress("another@test.com");

      // Act
      var builder = new EmailDestinationBuilder(fromAddress, new[] { toAddress, anotherAddress }, ServerName);
      string result = builder.Build();

      // Assert
      var expected =
        string.Format(
          "Type=Mail;From={0};To={1},{2};UseAttachment={3};SmtpServer={4};UseSSL=yes;Port=465;UseAuthentication=no;", 
          fromAddress.Address, 
          toAddress.Address, 
          anotherAddress.Address, 
          "false", 
          ServerName);

      Assert.Equal(expected, result);
    }
  }
}