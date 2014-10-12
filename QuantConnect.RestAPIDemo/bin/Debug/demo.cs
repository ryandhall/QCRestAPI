using System;
using System.Collections;
using System.Collections.Generic; 
using QuantConnect.Securities;  
using QuantConnect.Models;   
 
namespace QuantConnect 
{   
    // Name your algorithm class anything, as long as it inherits QCAlgorithm
    public class BasicTemplateAlgorithm : QCAlgorithm 
    {
        //Initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            SetStartDate(2010, 1, 1);          
            SetEndDate(DateTime.Now.Date.AddDays(-1)); 
            SetCash(25000);
            AddSecurity(SecurityType.Equity, "MSFT", Resolution.Minute);
        }

        //Data Event Handler: New data arrives here. "TradeBars" type is a dictionary of strings so you can access it by symbol.
        public void OnData(TradeBars data) 
        {   
            if (!Portfolio.HoldStock) 
            {
                Order("MSFT", (int)Math.Floor(Portfolio.Cash / data["MSFT"].Close) );
                Debug("Debug Purchased MSFT");
            }
        }
    }
}
