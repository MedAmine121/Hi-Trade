using FluentAssertions;
using FluentValidation;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hi_Trade.Tests
{
    [TestClass]
    public sealed class GetPortfolioTransactionsValidatorTests
    {
        private GetPortfolioTransactionsValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _validator = new GetPortfolioTransactionsValidator();
        }

        #region PortfolioId Validation

        [TestMethod]
        public async Task Validate_WithValidPortfolioId_ReturnsSuccess()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "test@example.com"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public async Task Validate_WithZeroPortfolioId_ReturnsFail()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 0,
                Email = "test@example.com"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PortfolioId");
        }

        [TestMethod]
        public async Task Validate_WithNegativePortfolioId_ReturnsFail()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = -5,
                Email = "test@example.com"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PortfolioId");
        }

        #endregion

        #region Email Validation

        [TestMethod]
        public async Task Validate_WithValidEmail_ReturnsSuccess()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "user@example.com"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public async Task Validate_WithEmptyEmail_ReturnsFail()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = ""
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [TestMethod]
        public async Task Validate_WithInvalidEmailFormat_ReturnsFail()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "invalid-email"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [TestMethod]
        public async Task Validate_WithEmailMissingLocalPart_ReturnsFail()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "@example.com"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [TestMethod]
        public async Task Validate_WithEmailMissingDomain_ReturnsFail()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "user@"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        #endregion

        #region Combined Validation

        [TestMethod]
        public async Task Validate_WithBothInvalidFields_ReturnsFail()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 0,
                Email = "invalid-email"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        [TestMethod]
        public async Task Validate_WithAllValidFields_ReturnsSuccess()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 42,
                Email = "valid.email@example.co.uk"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public async Task Validate_WithLargePortfolioId_ReturnsSuccess()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = int.MaxValue,
                Email = "test@example.com"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public async Task Validate_WithWhitespaceEmail_ReturnsFail()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "   "
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [TestMethod]
        public async Task Validate_WithComplexValidEmail_ReturnsSuccess()
        {
            // Arrange
            var request = new GetPortfolioTransactionsRequest
            {
                PortfolioId = 1,
                Email = "user.name+tag@example.co.uk"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion
    }
}
