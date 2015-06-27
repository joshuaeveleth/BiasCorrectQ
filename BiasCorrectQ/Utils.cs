﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiasCorrectQ
{
static class Utils
{
    internal static List<double> ComputeCDF(List<double> values, out List<double> sorted_values)
    {
        var rval = new List<double> { };

        //copy values and sort
        var vals = new List<double>(values);
        vals.Sort();
        vals.Reverse();
        sorted_values = vals;

        int count = 1;
        foreach (var val in vals)
        {
            /* Statistical Downscaling Techniques for Global Climate Model
             * Simulations of Temperature and Precipitation with Application to
             * Water Resources Planning Studies
             *     Alan F. Hamlet
             *     Eric P. Salathé
             *     Pablo Carrasco
             *
             * unbiased quantile estimator is used to assign a plotting
             * position to the data based on the Cunnane formulation
             * (Stedinger et al. 1993):15€
             *     q = (i − 0.4) / (n + 0.2)
             * where q is the estimated quantile (or probability of exceedance)
             * for the data value of rank position i (rank position 1 is the highest value),
             * for a total sample size of n
             */
            rval.Add((count - 0.4) / (vals.Count + 0.2));
            count++;
        }

        return rval;
    }

    internal static List<double> GetWYAnnualAverages(List<Point> flow)
    {
        var annualData = GetWYAnnualData(flow);

        // get average for each year
        var values = new List<double> { };
        foreach (var item in annualData)
        {
            values.Add(item.Average());
        }

        return values;
    }

    private static List<List<double>> GetWYAnnualData(List<Point> flow)
    {
        int startYear = flow[0].Date.Year;
        int endYear = flow[flow.Count - 1].Date.Year;

        var annualData = new List<List<double>> { };
        for (int i = 0; i < (endYear - startYear); i++)
        {
            annualData.Add(new List<double> { });
        }

        // add data for the water year
        foreach (Point pt in flow)
        {
            if (pt.Date.Month < 10)
            {
                annualData[pt.Date.Year - startYear - 1].Add(pt.Value);
            }
            else
            {
                annualData[pt.Date.Year - startYear].Add(pt.Value);
            }
        }
        return annualData;
    }


    internal static List<double> GetWYAnnualVolumes(List<Point> flow)
    {
        var annualData = GetWYAnnualData(flow);

        // get sum for each year
        var values = new List<double> { };
        foreach (var item in annualData)
        {
            double sum = 0;
            foreach (var val in item)
            {
                sum += val;
            }
            values.Add(sum);
        }

        return values;
    }

    internal static List<Point> GetMeanSummaryHydrograph(List<Point> flow)
    {
        var rval = new List<Point> { };

        //initial new list to hold monthly values, index = month -1
        var monthData = new List<List<double>> { };
        for (int i = 0; i < 12; i++)
        {
            monthData.Add(new List<double> { });
        }

        //add monthly data
        foreach (var pt in flow)
        {
            monthData[pt.Date.Month - 1].Add(pt.Value);
        }

        //fill in rval with mean of month values, arbitrary dates of 10/1/1999 - 9/1/2000
        int[] wy_months = { 10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        foreach (var month in wy_months)
        {
            int year = (month > 9) ? 1999 : 2000;
            rval.Add(new Point(new DateTime(year, month, 1), monthData[month - 1].Average()));
        }

        return rval;
    }

    internal static void TruncateToWYs(List<Point> data)
    {
        //truncate beginning of data until October is found
        foreach (var pt in data.ToList())
        {
            if (pt.Date.Month != 10)
            {
                data.Remove(pt);
            }
            else
            {
                break;
            }
        }

        //truncate end of data until September is found
        for (int i = data.Count - 1; i >= 0; i--)
        {
            var pt = data[i];
            if (pt.Date.Month != 9)
            {
                data.Remove(pt);
            }
            else
            {
                break;
            }
        }
    }

} //namespace
} //class
