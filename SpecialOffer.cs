using S10274277E_Assignment;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Student Number: S10274277E
   Student Name:   Huang Enyu (Solo)
   Partner Name:   -
*/

namespace S10274277E_Assignment
{

    public class SpecialOffer
    {
        public string OfferCode { get; }
        public string OfferDesc { get; }
        public double Discount { get; }

        public SpecialOffer(string offerCode, string offerDesc, double discount)
        {
            OfferCode = offerCode;
            OfferDesc = offerDesc;
            Discount = discount;
        }

        public override string ToString()
        {
            return $"{OfferCode} - {OfferDesc} ({Discount}%)";
        }
    }
}




