using System;
using System.Collections.Generic;
using System.Drawing; // Required for graphics operations
using System.IO;
using System.Linq;
using System.Windows.Forms; // Required for manipulating form controls
using System.Windows.Input;

/// <summary>
/// This class is responsible for parsing and executing a set of drawing commands
/// on a PictureBox control's canvas.
/// </summary>

public class CommandParser
{
    static Dictionary<string,int> variables=new Dictionary<string,int>();
    
    
    private Graphics graphics; // Graphics object to perform drawing
    private Pen currentPen;  // Pen to draw shapes and lines
    /// <summary>
    /// returns the object of currentpen
    /// </summary>
    public Pen CurrentPen // Public property to expose the currentPen
    {
        get { return currentPen; }
    }
   /// <summary>
   /// returns current pen color
   /// </summary>
    public Color CurrentPenColor // Public property to expose the color of the currentPen
    {
        get { return currentPen.Color; }
    }
    /// <summary>
    /// returns or sets currentposition of pointer
    /// </summary>
    public Point CurrentPosition { get; set; }

    private bool fillMode; // Indicates whether shapes should be filled
    private Point currentPos; // Current position of the "pen" on the canvas
    private Bitmap canvas; // Bitmap that serves as the drawing surface
    private Action invalidatePictureBox; // Delegate to invalidate the PictureBox
    private PictureBox pictureBox; // The PictureBox control used for drawing

    /// <summary>
    /// Constructor initializing the CommandParser with a PictureBox.
    /// </summary>
    /// <param name="pictureBox">The PictureBox control where drawings will be rendered.</param>
    /// <param name="invalidateAction">The action to invalidate the PictureBox for updates.</param>
    public CommandParser(PictureBox pictureBox, Action invalidateAction)
    {
        this.pictureBox = pictureBox;
        canvas = new Bitmap(pictureBox.Width, pictureBox.Height); // Initialize the canvas with the size of the PictureBox
        graphics = Graphics.FromImage(canvas); // Get a Graphics object to draw on the bitmap
        pictureBox.Image = canvas; // Set the PictureBox's Image property to the bitmap
        currentPen = new Pen(Color.Black); // Initialize the pen with a default color
        fillMode = false; // Default to no fill mode
        currentPos = new Point(0, 0); // Start at the top-left corner
        invalidatePictureBox = invalidateAction; // Set the delegate for invalidating the PictureBox
    }

    // Resets the current pen position to the top-left corner

    private void ResetPenPosition()
    {
        currentPos = new Point(0, 0);
    }


    /// <summary>
    /// Evaluates the given mathematical expression and assigns the result to a variable.
    /// </summary>
    /// <param name="variableName">
    /// The name of the variable to which the result will be assigned.
    /// </param>
    /// <param name="expression">
    /// The mathematical expression to be evaluated. The expression can contain addition, subtraction,
    /// multiplication, and division operations.
    /// </param>
    /// <remarks>
    /// The supported operations are '+', '-', '*', and '/'.
    /// </remarks>
    /// <seealso cref="GetVariableValue(string)"/>
    static void EvaluateAndAssignVariable(string variableName, string expression)
    {
        // Split the expression into parts
        string[] parts = expression.Split(new char[] { '+', '-', '*', '/' }, StringSplitOptions.RemoveEmptyEntries);

        // Evaluate the expression
        int result = GetVariableValue(parts[0].Trim().ToLower());

        for (int i = 0; i < parts.Length-1; i += 1)
        {
            int operand = GetVariableValue(parts[i + 1].ToLower());
            if (expression.Contains("+"))
            {
                result += operand;
            }
            else if (expression.Contains("-"))
            {
                result -= operand;
            }
            else if (expression.Contains("*"))
            {
                result *= operand;
            }
            else if (expression.Contains("/"))
            {
                result /= operand;
            }
        }

        // Assign the result to the variable
        variables[variableName.ToLower()] = result;
    }

    /// <summary>
    /// Gets the value of the specified variable.
    /// </summary>
    /// <param name="variableName">The name of the variable whose value is to be retrieved.</param>
    /// <returns>
    /// The value of the specified variable. If the variable does not exist or its value cannot be determined,
    /// the method returns a default value for the data type (0 for numeric types, null for reference types).
    /// </returns>
    static int GetVariableValue(string variableName)
    {
        // If the variable is a number, parse it directly
        if (int.TryParse(variableName, out int value))
        {
            return value;
        }

        // If the variable exists, return its value; otherwise, return 0
        return variables.TryGetValue(variableName.ToLower(), out value) ? value : 0;
    }

    /// <summary>
    /// Executes the statements inside an "if" block based on the specified condition.
    /// </summary>
    /// <param name="statement">
    /// The if statement to be evaluated. It should be in the format "variable1 operator variable2",
    /// where operator can be "==", "<", ">", "<=", ">=", or "!=".
    /// </param>
    /// <param name="insideIfStatements">
    /// A list of statements inside the "if" block to be executed if the condition is true.
    /// </param>
    /// <remarks>
    /// The method evaluates the specified condition and executes the statements inside the "if" block
    /// if the condition is true. Supported operators are "==", "<", ">", "<=", ">=", and "!=".
    /// </remarks>
    /// <seealso cref="GetVariableValue(string)"/>
    /// <seealso cref="Interpreter(string)"/>

    void ExecuteIfStatement(string statement, List<string> insideIfStatements)
    {
        string[] parts = statement.Split(new string[] { "==", "<", ">", "<=", ">=" ,"!="}, StringSplitOptions.None);

        if (parts.Length != 2)
        {
            // Invalid if statement format
            return;
        }
        parts[0]=parts[0].Split(' ')[1];
        string variable1 = parts[0].Trim();
        string variable2 = parts[1].Trim();

        // Get the values of the variables
        int value1 = GetVariableValue(variable1);
        int value2 = GetVariableValue(variable2);

        // Determine the comparison result based on the operator
        bool conditionMet = false;

        if (statement.Contains("=="))
        {
            conditionMet = (value1 == value2);
        }
        else if (statement.Contains("<"))
        {
            conditionMet = (value1 < value2);
        }
        else if (statement.Contains(">"))
        {
            conditionMet = (value1 > value2);
        }
        else if (statement.Contains("<="))
        {
            conditionMet = (value1 <= value2);
        }
        else if (statement.Contains(">="))
        {
            conditionMet = (value1 >= value2);
        }
        else if (statement.Contains("!="))
        {
            conditionMet=(value1 != value2);
        }

        // Execute the statements inside the if block if the condition is true
        if (conditionMet)
        {
            insideIfStatements.Remove(statement);
            Interpreter(string.Join("\n",insideIfStatements));
        }
    }

    /// <summary>
    /// Executes a while loop based on the specified condition.
    /// </summary>
    /// <param name="statement">
    /// The while loop condition. It should be in the format "variable1 operator variable2",
    /// where operator can be "==", "<", ">", "<=", ">=", or "!=".
    /// </param>
    /// <param name="insideWhile">
    /// A list of statements inside the while loop to be repeatedly executed while the condition is true.
    /// </param>
    /// <remarks>
    /// The method evaluates the specified condition and repeatedly executes the statements inside the while loop
    /// as long as the condition is true. Supported operators are "==", "<", ">", "<=", ">=", and "!=".
    /// </remarks>
    /// <seealso cref="GetVariableValue(string)"/>
    /// <seealso cref="Interpreter(string)"/>

    void ExecuteWhileLoop(string statement, List<string> insideWhile)
    {
        string[] parts = statement.Split(new string[] { "==", "<", ">", "<=", ">=", "!=" }, StringSplitOptions.None);
        parts[0] = parts[0].Split(' ')[1];
        if (parts.Length != 2)
        {
            // Invalid if statement format
            return;
        }

        bool conditionMet = true;
        while (conditionMet)
        {
            
            string variable1 = parts[0].Trim();
            string variable2 = parts[1].Trim();

            // Get the values of the variables
            int value1 = GetVariableValue(variable1);
            int value2 = GetVariableValue(variable2);

            // Determine the comparison result based on the operator
            conditionMet = false;

            if (statement.Contains("=="))
            {
                conditionMet = (value1 == value2);
            }
            else if (statement.Contains("<"))
            {
                conditionMet = (value1 < value2);
            }
            else if (statement.Contains(">"))
            {
                conditionMet = (value1 > value2);
            }
            else if (statement.Contains("<="))
            {
                conditionMet = (value1 <= value2);
            }
            else if (statement.Contains(">="))
            {
                conditionMet = (value1 >= value2);
            }
            else if (statement.Contains("!="))
            {
                conditionMet = (value1 != value2);
            }
            // Execute the statements inside the if block if the condition is true
            insideWhile.Remove(statement);
            Interpreter(string.Join("\n", insideWhile));
        }

        
        
    }

    /// <summary>
    /// Interprets and executes a series of commands provided as a string.
    /// </summary>
    /// <param name="c">
    /// The string containing multiple commands separated by newline characters.
    /// </param>
    /// <remarks>
    /// The method interprets each command, supporting "if" statements and "while" loops,
    /// and executes the specified actions accordingly.
    /// </remarks>
    /// <seealso cref="ExecuteIfStatement(string, List{string})"/>
    /// <seealso cref="ExecuteWhileLoop(string, List{string})"/>
    public void Interpreter(string c)
    {
        
        List<string> ifList = new List<string>();
        List<string> whileList = new List<string>();
        bool isInIf = false;
        bool isInWhile = false;
        string[] commands = c.Split('\n');
        int countendloop = 0;
        int countWhile = 0;
        int countif = 0;
        int countendif = 0;

        foreach (string comman in commands)
        {
            countWhile = 0;
            string command = comman.TrimStart();
            countif = 0;
            string[] part = command.Split('=');
            // Split the command into parts and identify the action
            var parts = command.Trim().Split(' ');
            var action = parts[0].ToLower();
            foreach (string cwhile in whileList)//this loop will keeps check if new while loop is added because we need same amount of while and endloop in our commands
            {
                if (cwhile.ToLower().Contains("while") && !cwhile.Contains("endloop"))
                {

                    countWhile += 1;

                }
            }
            foreach (string cwhile in ifList)//this loop will keeps check if new if is added because we need same amount of if and endif in our commands
            {
                if (cwhile.Contains("if") && !cwhile.Contains("endif"))
                {

                    countif += 1;

                }
            }

            // Switch statement to handle different commands
            if (isInIf == true && action != "endif")//if ifinIf is true then we will collect each command in a list then pass that list to execute seperatly if condition is true
            {
                ifList.Add(command);
            }
            else if (isInWhile == true && action != "endloop")//same goes for while but it will keep on iterating till condition is not false
            {
                whileList.Add(command);
                
            }
            // Handle variable assignments
            else if (part.Length == 2)//it is splited using = so if it has length =2 then it is a variable operation
            {
                string variableName = part[0].Trim();
                string expression = part[1].Trim();

                EvaluateAndAssignVariable(variableName.ToLower(), expression);
            }
            else
            {
                
                switch (action) //action contains first word of any command so it states the action to be perfomed
                {
                    case ""://if empty line then pass
                        break;
                    case " ":
                        break;
                    case "moveto":
                        MoveTo(parts);
                        break;
                    case "drawto":
                        DrawTo(parts);
                        break;
                    case "rectangle"://passing every commands words so method will it self seperate it 
                        DrawRectangle(new string[] { action, GetVariableValue(parts[1]).ToString(), GetVariableValue(parts[2]).ToString() });
                        break;
                    case "circle":
                        DrawCircle(new string[] { action, GetVariableValue(parts[1]).ToString() });
                        break;
                    case "triangle":
                        DrawTriangle(new string[] { action, GetVariableValue(parts[1]).ToString(), GetVariableValue(parts[2]).ToString() });
                        break;
                    case "pen":
                        ChangePenColor(parts);
                        break;
                    case "fill":
                        SetFillMode(parts);
                        break;
                    case "clear":
                        ClearDrawingArea();
                        break;
                    case "reset":
                        ResetPenPosition();
                        break;
                    case "if": //if action is if then isinIF will be true and program will keep on adding commands to ifList till it doesnt find endif
                        isInIf = true;
                        ifList.Add(command);
                        break;
                    case "endif":
                        if (countif != countendif + 1)//if we have more than 1 if then it will wait for 1st if's endif to come 
                        {
                            ifList.Add(command);
                            countendif += 1;
                        }
                        else
                        {//the moment it get endif of everyif then it will pass the iflist to executeifStatement that will use iflist and statement to run every command
                            isInIf = false;
                            string ifstatement = ifList[0];
                            ExecuteIfStatement(ifstatement, ifList);
                            ifList = new List<string>();
                        }
                    break;
                    case "while"://if action is while then isinWhile will be true and program will keep on adding commands to whileList till it doesnt find endloop
                        isInWhile = true;
                        whileList.Add(command);
                        break;
                    case "endloop":

                        if (countWhile != countendloop+1)//if we have more than 1 while then it will wait for 1st while's endloop to come 
                        {
                            whileList.Add(command);
                            countendloop += 1;
                        }
                        else
                        {//the moment it get endloop of every while then it will pass the whilelist to executeWhileStatment that will use whilelist and statement to run every command in loop
                            isInWhile = false;
                            string whileStatement = whileList[0];
                            ExecuteWhileLoop(whileStatement, whileList);
                            whileList=new List<string>();
                        }
                            
                        break;
                    default:
                        throw new InvalidOperationException("Unknown command.");

                }
                
                pictureBox.Image = canvas;
                InvalidatePictureBox();
            }


            
        }
       
    }

    private void ChangePenColor(string[] parts)
    {
        if (parts.Length != 2) throw new ArgumentException("Pen command expects one parameter: color name.");

        // Change the current pen color
        Color color;
        switch (parts[1].ToLower())
        {
            case "red":
                color = Color.Red;
                break;
            case "green":
                color = Color.Green;
                break;
            case "blue":
                color = Color.Blue;
                break;
            
            default:
                throw new ArgumentException("Invalid color specified.");
        }
       
        var newPen = new Pen(color);
        currentPen.Dispose(); // Dispose of the old pen
        currentPen = newPen;
    }
   
    /// <summary>
    /// Sets the fill mode based on the provided command parameter.
    /// </summary>
    /// <param name="parts">
    /// An array containing two parts: the fill command and the parameter ("on" or "off").
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the number of parts in the input array is not equal to two or if an invalid fill mode is specified.
    /// </exception>

    private void SetFillMode(string[] parts)
    {
        if (parts.Length != 2) throw new ArgumentException("Fill command expects one parameter: on or off.");

        // Set the fill mode
        switch (parts[1].ToLower())
        {
            case "on":
                fillMode = true;
                MessageBox.Show("Fill mode is now ON", "Fill Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            case "off":
                fillMode = false;
                MessageBox.Show("Fill mode is now OFF", "Fill Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            default:
                throw new ArgumentException("Invalid fill mode specified. Use 'on' or 'off'.");
        }
    }

    //function to refresh the box
    private void InvalidatePictureBox()
    {
        pictureBox.Invalidate();
    }

    /// <summary>
    /// Moves the current position to the specified coordinates (x, y).
    /// </summary>
    /// <param name="parts">
    /// An array containing three parts: "moveto", "x", and "y".
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the number of parts in the input array is not equal to three.
    /// </exception>
    /// <param name="parts">The command parts containing the "moveto" keyword and the x, y coordinates.</param>
    public void MoveTo(string[] parts)
    {
        // Expecting parts to be ["moveto", "x", "y"]
        if (parts.Length != 3) throw new ArgumentException("MoveTo expects two parameters: x and y coordinates.");

        int x = int.Parse(parts[1]);
        int y = int.Parse(parts[2]);
        currentPos = new Point(x, y); // Update current position

        // Draw a small circle or dot to represent the pen
        DrawPenPosition();

    }
    // This function is to show where the current pointer is
    private void DrawPenPosition()
    {
        const int penSize = 2; // Size of the pen position indicator
        graphics.FillEllipse(currentPen.Brush, currentPos.X - penSize / 2, currentPos.Y - penSize / 2, penSize, penSize);
        InvalidatePictureBox();
    }

    /// <summary>
    /// Draws a line from the current position to the specified coordinates (x, y).
    /// </summary>
    /// <param name="parts">
    /// An array containing three parts: "drawto", "x", and "y".
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the number of parts in the input array is not equal to three.
    /// </exception>
    private void DrawTo(string[] parts)
    {
        // The command is expected to have exactly three parts: "drawto", "x", "y"
        if (parts.Length != 3) throw new ArgumentException("DrawTo expects two parameters: x and y coordinates.");

        int x = int.Parse(parts[1]);
        int y = int.Parse(parts[2]);
        // Draws a line from the current position to the new position (x, y)
        graphics.DrawLine(currentPen, currentPos, new Point(x, y));
        currentPos = new Point(x, y); // Updates the current position to the new position after drawing the line
    }

    /// <summary>
    /// Draws a rectangle with the specified width and height.
    /// </summary>
    /// <param name="parts">
    /// An array containing three parts: "rectangle", "width", and "height".
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the number of parts in the input array is not equal to three.
    /// </exception>
    /// <param name="parts">The command parts containing the "rectangle" keyword, width, and height.</param>
    private void DrawRectangle(string[] parts)
    {
        if (parts.Length != 3) throw new ArgumentException("Rectangle expects two parameters: width and height.");

        int width = int.Parse(parts[1]);
        int height = int.Parse(parts[2]);

        if (fillMode)
        {
            // Fills the rectangle with the current pen's color
            using (var brush = new SolidBrush(currentPen.Color))
            {
                graphics.FillRectangle(brush, currentPos.X, currentPos.Y, width, height);
            }
        }
        else
        {
            // Draws only the rectangle's border
            graphics.DrawRectangle(currentPen, currentPos.X, currentPos.Y, width, height);
            
        }
    }
    /// <summary>
    /// Draws a triangle with the specified width and height.
    /// </summary>
    /// <param name="parts">
    /// An array containing three parts: "triangle", "width", and "height".
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the number of parts in the input array is not equal to three.
    /// </exception>
    /// <param name="parts">The command parts containing the "triangle" keyword, width, and height.</param>
    private void DrawTriangle(string[] parts)
    {

        if (parts.Length != 3) throw new ArgumentException("Triangle expects two parameters: width and height.");

        int width = int.Parse(parts[1]);
        int height = int.Parse(parts[2]);

        // Calculate the vertices of the triangle
        Point[] trianglePoints = new Point[]
        {
            new Point(currentPos.X, currentPos.Y + height),                  // Bottom-left
            new Point(currentPos.X + width, currentPos.Y + height),         // Bottom-right
            new Point(currentPos.X + width / 2, currentPos.Y)               // Top-center
        };

        if (fillMode)
        {
            // Fills the triangle with the current pen's color
            using (var brush = new SolidBrush(currentPen.Color))
            {
                graphics.FillPolygon(brush, trianglePoints);
            }
        }
        else
        {
            // Draws only the triangle's border
            graphics.DrawPolygon(currentPen, trianglePoints);
        }

    }



    /// <summary>
    /// Draws a circle with the specified radius.
    /// </summary>
    /// <param name="parts">
    /// An array containing two parts: "circle" and "radius".
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the number of parts in the input array is not equal to two.
    /// </exception>
    /// <param name="parts">The command parts containing the "circle" keyword and the radius.</param>
    private void DrawCircle(string[] parts)
    {
        if (parts.Length != 2) throw new ArgumentException("Circle expects one parameter: radius.");

        int radius = int.Parse(parts[1]);

        if (fillMode)
        {
            // Fills the ellipse with the current pen's color
            using (var brush = new SolidBrush(currentPen.Color))
            {
                graphics.FillEllipse(brush, currentPos.X - radius, currentPos.Y - radius, radius * 2, radius * 2);
            }
        }
        else
        {
            // Draws only the ellipse's border
            graphics.DrawEllipse(currentPen, currentPos.X - radius, currentPos.Y - radius, radius * 2, radius * 2);
        }
    }
    /// <summary>
    /// Clears the entire drawing area, resetting it to white.
    /// </summary>
    private void ClearDrawingArea()
    {
        // Clears the canvas to white, effectively erasing any previous drawing
        graphics.Clear(Color.White);
    }

    /// <summary>
    /// Checks the syntax of a series of commands provided as a string.
    /// </summary>
    /// <param name="c">
    /// The string containing multiple commands separated by newline characters.
    /// </param>
    /// <returns>
    /// A string containing information about any syntax errors found during the analysis.
    /// </returns>
    /// <remarks>
    /// The method checks the syntax of each command, including specific commands such as "moveto", "drawto", "line",
    /// "rectangle", "circle", "triangle", "pen", "fill", "clear", "reset", "if", "endif", "while", "endloop", and variable assignments.
    /// </remarks>
    /// <seealso cref="CheckMoveAndDrawToSyntax(string[])"/>
    /// <seealso cref="CheckLineSyntax(string[])"/>
    /// <seealso cref="CheckRectangleSyntax(string[])"/>
    /// <seealso cref="CheckCircleSyntax(string[])"/>
    /// <seealso cref="CheckTriangleSyntax(string[])"/>
    /// <seealso cref="CheckPenSyntax(string[])"/>
    /// <seealso cref="CheckFillSyntax(string[])"/>
    /// <seealso cref="CheckIf(string)"/>
    /// <seealso cref="CheckWhile(string)"/>
    public string CheckSyntax(string c)
    {
        int countendloop = 0;
        int countWhile = 0;
        int countif = 0;
        int countendif = 0;
        string result="";
        string[] commands = c.Split('\n');
        foreach (string comman in commands)
        {
            string command = comman.TrimStart();
            string[] part = command.Split('=');
            // Split the command into parts and identify the action
            var parts = command.Trim().Split(' ');
            var action = parts[0].ToLower();


            switch (action)
            {
                case "":
                case " ":
                    break;
                case "moveto":
                case "drawto":
                    string r = CheckMoveAndDrawToSyntax(parts);
                    if (r != null)
                        result += "\n" + r; r = null;
                    break;
                case "line":
                    r = CheckLineSyntax(parts);
                    if(r!=null)
                        result += "\n" + r; r = null;
                    break;
                case "rectangle":
                    r = CheckRectangleSyntax(parts);
                    if (r != null)
                        result += "\n" + r; r = null;
                    break;
                case "circle":
                    r = CheckCircleSyntax(parts);
                    if (r != null)
                        result += "\n" + r; r = null;
                    break;
                case "triangle":
                    r = CheckTriangleSyntax(parts);
                    if (r != null)
                        result += "\n" + r; r = null;
                    break;
                case "pen":
                    r = CheckPenSyntax(parts);
                    if (r != null)
                        result += "\n" + r; r = null;
                    break;
                case "fill":
                    r = CheckFillSyntax(parts);
                    if (r != null)
                        result += "\n" + r; r = null;
                    break;
                case "clear":
                case "reset":
                    // just check for the correct length
                    if (parts.Length != 1)
                    {
                        result += "\n" + $"'{action}' command does not take parameters.";
                    }
                    break;
                case "if":
                    r = CheckIf(command);
                    if (r != null)
                        result += "\n" + r;
                    countif++;
                    break;
                case "endif":
                    countendif++;
                    break;
                case "while":
                    r = CheckWhile(command);
                    if (r != null)
                        result += "\n" + r;
                    countWhile++;
                    break;
                case "endloop":

                    countendloop++;

                    break;
                default:
                    if (command.Contains("="))
                    {
                        string[] var = command.Split('=');
                        string[] data = var[1].Split(new string[] { "+", "-", "*", "/"}, StringSplitOptions.None);
                        foreach (string d in data)
                        {
                            if (d != "0")
                            {
                                if (!variables.ContainsKey(d))
                                {
                                    int integer = 0;
                                    if (!int.TryParse(d,out integer))
                                    {
                                        result += "\n" + " Syntax Error at: " + command;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        result += "\n" + $"Unknown command: '{action}'.";
                    }
                    
                    break;
            }
            
        }
        if (countWhile != countendloop)
        {
            result += "\nWhile not implemented correctly";
        }
        if (countif != countendif)
        {
            result += "\nIF not implemented correctly";
        }

              
        return result;
    }

    /// <summary>
    /// Validates the syntax for 'moveto' and 'drawto' commands.
    /// </summary>
    /// <param name="parts">Array containing the command parts to validate.</param>
    /// <exception cref="FormatException">Thrown when the syntax does not match the expected format.</exception>
    private string CheckMoveAndDrawToSyntax(string[] parts)
    {
        // Validates that the command has three parts and that the x and y coordinates are integers
        if (parts.Length != 3 || GetVariableValue(parts[1]).GetType().ToString() !=  "System.Int32" || GetVariableValue(parts[1]).GetType().ToString() !=  "System.Int32")
        {
            return $"Invalid syntax for {parts[0]}. Correct syntax: '{parts[0]} x y'.";
        }
        else return null;
    }

    /// <summary>
    /// Checks the specified command and returns a string indicating whether the command is associated with a while loop.
    /// </summary>
    /// <param name="command">The command to be checked.</param>
    /// <returns>
    /// A string indicating the type of the command:
    /// - "While" if the command is associated with a while loop.
    /// - "NotWhile" if the command is not associated with a while loop.
    /// </returns>
    private string CheckWhile(string command)
    {
        string[] parts = command.Split(new string[] { "==", "<", ">", "<=", ">=", "!=" }, StringSplitOptions.None);

        if (parts.Length != 2)
        {
            // Invalid if statement format
            return "Invalid syntax. use while var1 > var2";
        }
        return null;
    }

    /// <summary>
    /// Checks the specified command and returns a string indicating whether the command is associated with an if statement.
    /// </summary>
    /// <param name="command">The command to be checked.</param>
    /// <returns>
    /// A string indicating the type of the command:
    /// - "If" if the command is associated with an if statement.
    /// - "NotIf" if the command is not associated with an if statement.
    /// </returns>
    private string CheckIf(string command)
    {
        string[] parts = command.Split(new string[] { "==", "<", ">", "<=", ">=", "!=" }, StringSplitOptions.None);

        if (parts.Length != 2)
        {
            // Invalid if statement format
            return "Invalid syntax. use if var1 > var2";
        }
        return null;
    }
    /// <summary>
    /// Validates the syntax for the 'rectangle' command.
    /// </summary>
    /// <param name="parts">Array containing the command parts to validate.</param>
    /// <exception cref="FormatException">Thrown when the syntax does not match the expected format.</exception>
    private string CheckRectangleSyntax(string[] parts)
    {
        // Validates that the command has three parts and that the width and height are integers
        if (parts.Length != 3 || GetVariableValue(parts[1]).GetType().ToString() != "System.Int32" || GetVariableValue(parts[1]).GetType().ToString() != "System.Int32")
        {
            return "Invalid syntax for rectangle. Correct syntax: 'rectangle width height'.";
        }
        else 
            return null;
    }
    /// <summary>
    /// Checks the syntax of the provided array of string parts to determine if it represents a valid triangle.
    /// </summary>
    /// <param name="parts">An array of string parts representing the components of a triangle.</param>
    /// <returns>
    /// A string indicating the syntax check result:
    /// - "Valid" if the string parts represent a valid triangle.
    /// - "Invalid" if the string parts do not form a valid triangle.
    /// </returns>
    private string CheckTriangleSyntax(string[] parts)
    {
        // Validates that the command has three parts and that the width and height are integers
        if (parts.Length != 3 || GetVariableValue(parts[1]).GetType().ToString() != "System.Int32" || GetVariableValue(parts[1]).GetType().ToString() != "System.Int32")
        {
            return "Invalid syntax for Triangle. Correct syntax: 'triangle width height'.";
        }
        else
            return null;
    }

    /// <summary>
    /// Validates the syntax for the 'circle' command.
    /// </summary>
    /// <param name="parts">Array containing the command parts to validate.</param>
    /// <exception cref="FormatException">Thrown when the syntax does not match the expected format.</exception>
    private string CheckCircleSyntax(string[] parts)
    {
        // Validates that the command has two parts and that the radius is an integer
        if (parts.Length != 2 || GetVariableValue(parts[1]).GetType().ToString() != "System.Int32")
        {
            return "Invalid syntax for circle. Correct syntax: 'circle radius'.";
        }
        else
            return null;
    }


    /// <summary>
    /// Validates the syntax for the 'line' command.
    /// </summary>
    /// <param name="parts">Array containing the command parts to validate.</param>
    /// <exception cref="FormatException">Thrown when the syntax does not match the expected format.</exception>
    private string CheckLineSyntax(string[] parts)
    {
        // Validates that the command has two parts and that the radius is an integer
        if (parts.Length != 2 || GetVariableValue(parts[1]).GetType().ToString() != "System.Int32")
        {
            MessageBox.Show(GetVariableValue(parts[1]).GetType().ToString());
            return "Invalid syntax for line. Correct syntax: 'line length'.";
            
            
        }
        else
            return null;
    }

    /// <summary>
    /// Validates the syntax for the 'pen' command.
    /// </summary>
    /// <param name="parts">Array containing the command parts to validate.</param>
    /// <exception cref="FormatException">Thrown when the syntax does not match the expected format or the color is not known.</exception>
    private string CheckPenSyntax(string[] parts)
    {
        // Validates that the command has two parts and that the specified color is known
        if (parts.Length != 2)
        {
            return "Invalid syntax for pen. Correct syntax: 'pen color'.";
        }

        // Check if the color is a valid named color
        if (!Color.FromName(parts[1]).IsKnownColor)
        {
            return $"'{parts[1]}' is not a known color.";
        }
        else { return null; }
    }

    /// <summary>
    /// Validates the syntax for the 'fill' command.
    /// </summary>
    /// <param name="parts">Array containing the command parts to validate.</param>
    /// <exception cref="FormatException">Thrown when the syntax does not match the expected format.</exception>
    private string CheckFillSyntax(string[] parts)
    {
        // Validates that the command has two parts and that the parameter is either 'on' or 'off'
        if (parts.Length != 2 || !(parts[1].ToLower() == "on" || parts[1].ToLower() == "off"))
        {
            return "Invalid syntax for fill. Correct syntax: 'fill on' or 'fill off'.";
        }
        else
            return null;
    }

    /// <summary>
    /// Opens and reads the content of an IPL file selected by the user through a file dialog.
    /// </summary>
    /// <returns>
    /// A string containing the content of the selected IPL file, or null if the file is not valid or does not exist.
    /// </returns>
    /// <remarks>
    /// The method displays an OpenFileDialog to allow the user to choose an IPL file. If the selected file has the ".ipl" extension,
    /// and it exists, the content of the file is read and returned as a string. If the file is not valid or does not exist,
    /// the method returns null. An error message is displayed if an invalid file type is selected.
    /// </remarks>
    public string OpenFile()
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "IPL files (*.ipl)|*.ipl";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Check if the selected file has the "IPL" extension
                if (System.IO.Path.GetExtension(openFileDialog.FileName).Equals(".ipl", StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(openFileDialog.FileName))
                    {
                        // Read the content of the file
                        return File.ReadAllText(openFileDialog.FileName);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    MessageBox.Show("Please select a file with the '.ipl' extension.", "Invalid File Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        return null; // Return null if no valid file is selected
    }

    /// <summary>
    /// Opens a file with the specified filename and returns a string representing the contents of the file.
    /// </summary>
    /// <param name="filename">The name of the file to be opened.</param>
    /// <returns>
    /// A string containing the contents of the opened file. If the file cannot be opened or an error occurs, returns an empty string.
    /// </returns>
    public string OpenFile(string filename)
    {
        if (File.Exists(filename))
        {
            // Read the content of the file
            return File.ReadAllText(filename);
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// Saves the provided content to an IPL file chosen by the user through a save file dialog.
    /// </summary>
    /// <param name="content">
    /// The content to be saved to the IPL file.
    /// </param>
    /// <remarks>
    /// The method displays a SaveFileDialog to allow the user to choose or specify a location for saving an IPL file.
    /// If the user confirms the save operation, the provided content is saved to the selected file. If the file already exists,
    /// it will be overwritten. The ".ipl" extension is added to the file name if not provided by the user.
    /// </remarks>
    /// <seealso cref="OpenFile"/>
    public void SaveIPLFile(string content)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "IPL files (*.ipl)|*.ipl";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    // Add ".ipl" extension if not provided by the user
                    if (!filePath.EndsWith(".ipl", StringComparison.OrdinalIgnoreCase))
                    {
                        filePath += ".ipl";
                    }

                    File.WriteAllText(filePath, content);
                    Console.WriteLine($"File saved: {filePath}");
                }
            }
        }

}



