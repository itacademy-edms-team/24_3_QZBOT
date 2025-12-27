using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;
using System.Security.Claims;
using WebTests.Controllers;
using WebTests.Data;
using WebTests.DTOs;
using WebTests.Models;
using Xunit;

namespace Tests {
    public class TestsControllerTests
    {
        #region CheckTestExists
        public class CheckTestExists
        {
            [Fact]
            public void CheckTestExists_ReturnsTrue_WhenTestExists()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(CheckTestExists_ReturnsTrue_WhenTestExists));

                context.Tests.Add(new Test
                {
                    Title = "C# Basics"
                });
                context.SaveChanges();

                var controller = new TestsController(context);

                // Act
                var result = controller.CheckTestExists("C# Basics");

                // Assert
                var request = Assert.IsType<OkObjectResult>(result);
            }

            [Fact]
            public void CheckTestExists_ReturnsFalse_WhenTestDoesNotExist()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(CheckTestExists_ReturnsFalse_WhenTestDoesNotExist));
                var controller = new TestsController(context);

                // Act
                var result = controller.CheckTestExists("Unknown test");

                // Assert
                var request = Assert.IsType<OkObjectResult>(result);
            }
        }
        #endregion




        #region GetTestByTitle
        public class GetTestByTitle
        {
            [Fact]
            public void GetTestByTitle_ReturnsNotNull_WhenTestExist()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(GetTestByTitle_ReturnsNotNull_WhenTestExist));
                context.Tests.Add(new Test
                {
                    Title = "C# Basics"
                });
                context.SaveChanges();
                var controller = new TestsController(context);

                // Act
                var result = controller.GetTestByTitle("C# Basics");

                // Assert
                var request = Assert.IsType<OkObjectResult>(result);
            }

            [Fact]
            public void GetTestByTitle_ReturnsNull_WhenTestDoesNotExist()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(GetTestByTitle_ReturnsNull_WhenTestDoesNotExist));
                var controller = new TestsController(context);

                // Act
                var result = controller.GetTestByTitle("unknown");

                // Assert
                var request = Assert.IsType<OkObjectResult>(result);
            }
        }
        #endregion




        #region GetTestById
        public class GetTestById
        {
            [Fact]
            public void GetTestById_ReturnsNotNull_WhenTestExist()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(GetTestById_ReturnsNotNull_WhenTestExist));
                context.Tests.Add(new Test
                {
                    Id = 1,
                    Title = "C# Basics"
                });
                context.SaveChanges();
                var controller = new TestsController(context);

                // Act
                var result = controller.GetTestById(1);

                // Assert
                var request = Assert.IsType<OkObjectResult>(result);
            }

            [Fact]
            public void GetTestById_ReturnsNull_WhenTestDoesNotExist()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(GetTestById_ReturnsNull_WhenTestDoesNotExist));
                var controller = new TestsController(context);

                // Act
                var result = controller.GetTestById(1);

                // Assert
                var request = Assert.IsType<OkObjectResult>(result);
            }
        }
        #endregion




        #region CheckAnswer
        public class CheckAnswer()
        {
            [Fact]
            public void CheckAnswer_ReturnsBadRequest_WhenDtoIsNull()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(CheckAnswer_ReturnsBadRequest_WhenDtoIsNull));
                var controller = new TestsController(context);

                // Act
                var result = controller.CheckAnswer(null);

                // Assert
                var request = Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public void CheckAnswer_ReturnsNotFound_WhenQuestionDoesNotExist()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(CheckAnswer_ReturnsNotFound_WhenQuestionDoesNotExist));
                var controller = new TestsController(context);

                var dto = new AnswerCheckDto
                {
                    Title = "C# Basics",
                    QuestionId = 1,
                    SelectedOptionIndex = 0
                };

                // Act
                var result = controller.CheckAnswer(dto);

                // Assert
                var request = Assert.IsType<NotFoundObjectResult>(result);
            }

            [Fact]
            public void CheckAnswer_ReturnsBadRequest_WhenOptionIndexIsOutOfRange()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(CheckAnswer_ReturnsBadRequest_WhenOptionIndexIsOutOfRange));

                var test = new Test
                {
                    Title = "C# Basics",
                    Questions = new List<Question>
                {
                    new Question
                    {
                        Options = new List<AnswerOption>
                        {
                            new AnswerOption { Text = "A", IsCorrect = true }
                        }
                    }
                }
                };
                context.Tests.Add(test);
                context.SaveChanges();

                var controller = new TestsController(context);

                var dto = new AnswerCheckDto
                {
                    Title = "C# Basics",
                    QuestionId = test.Questions.First().Id,
                    SelectedOptionIndex = 5
                };

                // Act
                var result = controller.CheckAnswer(dto);

                // Assert
                var request = Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public void CheckAnswer_ReturnsTrue_WhenAnswerIsCorrect()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(CheckAnswer_ReturnsTrue_WhenAnswerIsCorrect));

                var test = new Test
                {
                    Title = "C# Basics",
                    Questions = new List<Question>
                {
                    new Question
                    {
                        Options = new List<AnswerOption>
                        {
                            new AnswerOption { Text = "A", IsCorrect = true },
                            new AnswerOption { Text = "B", IsCorrect = false }
                        }
                    }
                }
                };

                context.Tests.Add(test);
                context.SaveChanges();

                var controller = new TestsController(context);

                var dto = new AnswerCheckDto
                {
                    Title = "C# Basics",
                    QuestionId = test.Questions.First().Id,
                    SelectedOptionIndex = 0
                };

                // Act
                var result = controller.CheckAnswer(dto);

                // Assert
                var request = Assert.IsType<OkObjectResult>(result);
            }

            [Fact]
            public void CheckAnswer_ReturnsFalse_WhenAnswerIsIncorrect()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(CheckAnswer_ReturnsFalse_WhenAnswerIsIncorrect));

                var test = new Test
                {
                    Title = "Basics",
                    Questions = new List<Question>
                {
                    new Question
                    {
                        Options = new List<AnswerOption>
                        {
                            new AnswerOption { Text = "A", IsCorrect = false },
                            new AnswerOption { Text = "B", IsCorrect = true }
                        }
                    }
                }
                };

                context.Tests.Add(test);
                context.SaveChanges();

                var controller = new TestsController(context);

                var dto = new AnswerCheckDto
                {
                    Title = "Basics",
                    QuestionId = test.Questions.First().Id,
                    SelectedOptionIndex = 0
                };

                // Act
                var result = controller.CheckAnswer(dto);

                // Assert
                var request = Assert.IsType<OkObjectResult>(result);
            }
        }
        #endregion




        #region AddTest
        public class AddTest()
        {
            [Fact]
            public async Task AddTest_ReturnsUnauthorized_WhenUserIsMissing()
            {
                // Arrange 
                var context = TestDbContextFactory.Create(nameof(AddTest_ReturnsUnauthorized_WhenUserIsMissing));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var dto = new TestDto
                {
                    Title = "Test",
                    Questions = new List<QuestionDto>
                    {
                        new QuestionDto
                        {
                            Text = "Q1",
                            Options = new List<AnswerOptionDto>
                            {
                                new AnswerOptionDto { Text = "A", IsCorrect = true }
                            }
                        }
                    }
                };

                // Act
                var result = await controller.AddTest(dto);

                //Assert
                var request = Assert.IsType<UnauthorizedResult>(result);
            }

            [Fact]
            public async Task AddTest_ReturnsBadRequest_WhenDtoIsNull()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(AddTest_ReturnsUnauthorized_WhenUserIsMissing));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var dto = new TestDto();

                // Act
                var result = await controller.AddTest(dto);

                // Assert
                var request = Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task AddTest_ReturnsBadRequest_WhenTitleIsNull()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(AddTest_ReturnsBadRequest_WhenTitleIsNull));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var dto = new TestDto
                {
                    Questions = new List<QuestionDto>
                    {
                        new QuestionDto
                        {
                            Text = "Q1",
                            Options = new List<AnswerOptionDto>
                            {
                                new AnswerOptionDto { Text = "A", IsCorrect = true }
                            }
                        }
                    }
                };

                // Act
                var result = await controller.AddTest(dto);

                // Assert
                var request = Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task AddTest_ReturnsBadRequest_WhenQuestionsAreNull()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(AddTest_ReturnsBadRequest_WhenQuestionsAreNull));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var dto = new TestDto
                {
                    Title = "Test",
                    Questions = new List<QuestionDto>()
                };

                // Act
                var result = await controller.AddTest(dto);

                // Assert
                var request = Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task AddTest_ReturnsCreatedAtAction_WhenAllCorrect()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(AddTest_ReturnsCreatedAtAction_WhenAllCorrect));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var dto = new TestDto
                {
                    Title = "Test",
                    Questions = new List<QuestionDto>
                    {
                        new QuestionDto
                        {
                            Text = "Q1",
                            Options = new List<AnswerOptionDto>
                            {
                                new AnswerOptionDto { Text = "A", IsCorrect = true }
                            }
                        }
                    }
                };

                // Act
                var result = await controller.AddTest(dto);

                // Assert
                var request = Assert.IsType<CreatedAtActionResult>(result);
            }
        }
        #endregion




        #region
        public class EditTest()
        {
            [Fact]
            public async Task EditTest_ReturnsBadRequest_WhenUpdatedIsNull()
            {
                // Arrange 
                var context = TestDbContextFactory.Create(nameof(EditTest_ReturnsBadRequest_WhenUpdatedIsNull));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var test = new Test
                {
                    Title = "Test",
                    Questions = new List<Question>(),
                    CreatorId = "user-2"
                };
                context.Tests.Add(test);
                await context.SaveChangesAsync();

                var updated = new TestDto();

                // Act
                var result = await controller.EditTest(1, updated);

                // Assert
                var request = Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task EditTest_ReturnsNotFound_WhenTestNotFound()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(EditTest_ReturnsNotFound_WhenTestNotFound));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var updated = new TestDto
                {
                    Title = "Test",
                    Questions = new List<QuestionDto>
                    {
                        new QuestionDto
                        {
                            Text = "Q1",
                            Options = new List<AnswerOptionDto>
                            {
                                new AnswerOptionDto { Text = "A", IsCorrect = true }
                            }
                        }
                    }
                };

                // Act
                var result = await controller.EditTest(1, updated);

                // Assert
                var request = Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task EditTest_ReturnsForbid_WhenNotCreator()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(EditTest_ReturnsForbid_WhenNotCreator));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var test = new Test
                {
                    Title = "Test",
                    Questions = new List<Question>(),
                    CreatorId = "user-2"
                };
                context.Tests.Add(test);
                await context.SaveChangesAsync();

                var updated = new TestDto
                {
                    Title = "Test",
                    Questions = new List<QuestionDto>
                    {
                        new QuestionDto
                        {
                            Text = "Q1",
                            Options = new List<AnswerOptionDto>
                            {
                                new AnswerOptionDto { Text = "A", IsCorrect = true }
                            }
                        }
                    }
                };

                // Act
                var result = await controller.EditTest(1, updated);

                // Assert
                var request = Assert.IsType<ForbidResult>(result);
            }

            [Fact]
            public async Task EditTest_ReturnsUnauthorized_WhenNotAuthorized()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(EditTest_ReturnsUnauthorized_WhenNotAuthorized));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var test = new Test
                {
                    Title = "Test",
                    Questions = new List<Question>(),
                    CreatorId = "user-2"
                };
                context.Tests.Add(test);
                await context.SaveChangesAsync();

                var updated = new TestDto
                {
                    Title = "Test",
                    Questions = new List<QuestionDto>
                    {
                        new QuestionDto
                        {
                            Text = "Q1",
                            Options = new List<AnswerOptionDto>
                            {
                                new AnswerOptionDto { Text = "A", IsCorrect = true }
                            }
                        }
                    }
                };

                // Act
                var result = await controller.EditTest(1, updated);

                // Assert
                var request = Assert.IsType<UnauthorizedResult>(result);
            }
        }
        #endregion



        #region PassTest
        public class PassTest
        {
            [Fact]
            public async Task PassTest_ReturnsUnauthorized_WhenUnauthorized()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(PassTest_ReturnsUnauthorized_WhenUnauthorized));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var test = new Test
                {
                    Title = "Test",
                    Questions = new List<Question>()
                };
                context.Tests.Add(test);
                await context.SaveChangesAsync();

                // Act
                var result = await controller.PassTest(test.Id, 2);

                // Assert
                var request = Assert.IsType<UnauthorizedResult>(result);
            }

            [Fact]
            public async Task PassTest_ReturnsNotFound_WhenTestNotFound()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(PassTest_ReturnsNotFound_WhenTestNotFound));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                // Act
                var result = await controller.PassTest(2, 1);

                // Assert
                var request = Assert.IsType<NotFoundObjectResult>(result);
            }

            [Fact]
            public async Task PassTest_ReturnsBadRequest_WhenAlreadyTried()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(PassTest_ReturnsBadRequest_WhenAlreadyTried));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var test = new Test
                {
                    Title = "Test",
                    Questions = new List<Question>()
                };
                context.Tests.Add(test);


                var userTest = new UserTest
                {
                    UserId = "user-1",
                    TestId = test.Id
                };
                context.UserTests.Add(userTest);
                await context.SaveChangesAsync();

                // Act
                var result = await controller.PassTest(test.Id, 1);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task PassTest_ReturnsOk_WhenAllOk()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(PassTest_ReturnsBadRequest_WhenAlreadyTried));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var test = new Test
                {
                    Title = "Test",
                    Questions = new List<Question>()
                };
                context.Tests.Add(test);
                await context.SaveChangesAsync();

                // Act
                var result = await controller.PassTest(test.Id, 1);

                // Assert
                Assert.IsType<OkObjectResult>(result);
            }
        }
        #endregion



        #region IsTestPassed
        public class IsTestPassed
        {
            [Fact]
            public async Task IsTestPassed_ReturnsUnauthorized_WhenUnauthorized()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(IsTestPassed_ReturnsUnauthorized_WhenUnauthorized));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                // Act
                var result = await controller.IsTestPassed(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }

            [Fact]
            public async Task IsTestPassed_ReturnsNotFound_WhenTestNotFound()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(IsTestPassed_ReturnsNotFound_WhenTestNotFound));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                // Act
                var result = await controller.IsTestPassed(1);
                
                // Assert
                Assert.IsType<NotFoundResult>(result);
            }

            [Fact]
            public async Task IsTestPassed_ReturnsOk_WhenAllOk()
            {
                // Arrange
                var context = TestDbContextFactory.Create(nameof(IsTestPassed_ReturnsOk_WhenAllOk));
                var controller = new TestsController(context);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, "user-1")
                            }, "TestAuth")
                        )
                    }
                };

                var test = new Test
                {
                    Title = "Test",
                    Questions = new List<Question>()
                };
                context.Tests.Add(test);

                var userTest = new UserTest
                {
                    UserId = "user-1",
                    TestId = test.Id
                };
                context.UserTests.Add(userTest);
                await context.SaveChangesAsync();

                // Act
                var result = await controller.IsTestPassed(test.Id);

                // Assert
                Assert.IsType<OkObjectResult>(result);
            }
        }
        #endregion
    }
}