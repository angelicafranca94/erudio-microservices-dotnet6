using Microsoft.AspNetCore.Mvc;

namespace RestWithASPNETUdemy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly ILogger<CalculatorController> _logger;

        public CalculatorController(ILogger<CalculatorController> logger)
        {
            _logger = logger;
        }

        [HttpGet("sum/{firstNumber}/{secondNumber}")]
        public IActionResult Soma(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var sum = ConvertToDecimal(firstNumber) + ConvertToDecimal(secondNumber);
                return Ok(sum.ToString());
            }
            return BadRequest("Invalid Input");
        }


        [HttpGet("sub/{firstNumber}/{secondNumber}")]
        public IActionResult Sub(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var sum = ConvertToDecimal(firstNumber) - ConvertToDecimal(secondNumber);
                return Ok(sum.ToString());
            }
            return BadRequest("Invalid Input");
        }

        [HttpGet("multi/{firstNumber}/{secondNumber}")]
        public IActionResult Multi(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var sum = ConvertToDecimal(firstNumber) * ConvertToDecimal(secondNumber);
                return Ok(sum.ToString());
            }
            return BadRequest("Invalid Input");
        }

        [HttpGet("div/{firstNumber}/{secondNumber}")]
        public IActionResult Div(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var sum = ConvertToDecimal(firstNumber) / ConvertToDecimal(secondNumber);
                return Ok(sum.ToString());
            }
            return BadRequest("Invalid Input");
        }

        [HttpGet("med/{firstNumber}/{secondNumber}")]
        public IActionResult Med(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var sum = ConvertToDecimal(firstNumber) + ConvertToDecimal(secondNumber);
                var media = sum / 2;
                return Ok(media.ToString());
            }
            return BadRequest("Invalid Input");
        }

        [HttpGet("quad/{firstNumber}")]
        public IActionResult Quad(string firstNumber)
        {
            if (IsNumeric(firstNumber))
            {
                var sum = Math.Sqrt(Convert.ToDouble(ConvertToDecimal(firstNumber)));
                return Ok(sum.ToString());
            }
            return BadRequest("Invalid Input");
        }

        private bool IsNumeric(string strNumber)
        {
            double number;

            bool isNumber = double.TryParse(strNumber, System.Globalization.NumberStyles.Any,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out number);

            return isNumber;
        }

        private decimal ConvertToDecimal(string strNumber)
        {
            decimal decimalValue;

            if (decimal.TryParse(strNumber, out decimalValue))
                return decimalValue;

            return 0;
        }


    }
}