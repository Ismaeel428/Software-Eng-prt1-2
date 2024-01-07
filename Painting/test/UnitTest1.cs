using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace test
{
    [TestClass]
    public class CommandParserTests
    {
        private PictureBox pictureBox;
        private CommandParser parser;

        [TestInitialize]
        public void Initialize()
        {
            // Create a new PictureBox for each test to ensure a clean state
            pictureBox = new PictureBox();
            Action invalidateAction = () => { };
            parser = new CommandParser(pictureBox, invalidateAction);
        }

        [TestMethod]
        public void TestPenCommandChangesPenColor()
        {
            // Act
            parser.Interpreter("pen red");

            // Assert
            Assert.AreEqual(Color.Red, parser.CurrentPenColor, "The pen color should change to red.");
        }

   
   

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidPenCommandThrowsException()
        {
            // Act
            parser.Interpreter("pen rainbow");
        }

   

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestUnknownCommandThrowsException()
        {
            // Act
            parser.Interpreter("unknowncommand 123");
        }

       
        //test method to draw a rectangle
        [TestMethod]
        public void TestDrawRectangleCommand()
        {
            // Arrange setting width and heigth of the pen location 
            int initialX = 10;
            int initialY = 10;
            parser.MoveTo(new string[] { "moveto", initialX.ToString(), initialY.ToString() });  //moveto the specific position to draw

            // Act
            parser.Interpreter("rectangle 50 100");  //calling the command with the height and width of the rectangle to create



        }
        //Testing Triangle
        [TestMethod]
        public void TestDrawTriangleCommand()
        {
            // Arrange
            int initialX = 30;
            int initialY = 30;
            parser.MoveTo(new string[] { "moveto", initialX.ToString(), initialY.ToString() });

            // Act
            parser.Interpreter("triangle 40 50 60");  //Executing the command to create a triangle with base 40 ....

            // Assert
            // Check for successful execution of the triangle command.
        }
        //Testing Circle
        [TestMethod]
        public void TestDrawCircleCommand()
        {
            // Arrange
            int initialX = 40;
            int initialY = 40;
            parser.MoveTo(new string[] { "moveto", initialX.ToString(), initialY.ToString() });

            // Act
            parser.Interpreter("circle 50");  //Executing the command to create a triangle with base 40 ....

            // Assert
            // Check for successful execution of the circle command.
        }

        [TestMethod]
        public void TestWhile()
        {
            // Act
            string commands=parser.OpenFile("while.ipl");
            parser.Interpreter(commands);
        }
        [TestMethod]
        public void TestIF()
        {
            // Act
            string commands = parser.OpenFile("ss.ipl");
            parser.Interpreter(commands);
        }
        [TestMethod]
        public void Test1()
        {
            // Act
            string commands = parser.OpenFile("test.ipl");
            parser.Interpreter(commands);
        }


    }
}
