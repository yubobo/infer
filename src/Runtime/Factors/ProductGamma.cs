// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.ML.Probabilistic.Factors
{
    using System;
    using Microsoft.ML.Probabilistic.Distributions;
    using Microsoft.ML.Probabilistic.Math;
    using Microsoft.ML.Probabilistic.Factors.Attributes;

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "Product", typeof(double), typeof(double))]
    [Quality(QualityBand.Preview)]
    public static class GammaProductOp
    {
        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="ProductAverageConditional(double, Gamma)"]/*'/>
        public static Gamma ProductAverageConditional(double A, [SkipIfUniform] Gamma B)
        {
            return GammaProductVmpOp.ProductAverageLogarithm(A, B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="ProductAverageConditional(Gamma, double)"]/*'/>
        public static Gamma ProductAverageConditional([SkipIfUniform] Gamma A, double B)
        {
            return ProductAverageConditional(B, A);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="ProductAverageConditional(double, double)"]/*'/>
        public static Gamma ProductAverageConditional(double a, double b)
        {
            return Gamma.PointMass(a * b);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="AAverageConditional(Gamma, double)"]/*'/>
        public static Gamma AAverageConditional([SkipIfUniform] Gamma Product, double B)
        {
            if (Product.IsPointMass)
                return AAverageConditional(Product.Point, B);
            if (B == 0)
                return Gamma.Uniform();
            // (ab)^(shape-1) exp(-rate*(ab))
            return Gamma.FromShapeAndRate(Product.Shape, Product.Rate * B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="AAverageConditional(double, double)"]/*'/>
        public static Gamma AAverageConditional(double Product, double B)
        {
            Gamma result = new Gamma();
            if (B == 0)
            {
                if (Product != 0)
                    throw new AllZeroException();
                result.SetToUniform();
            }
            else if ((Product > 0) != (B > 0))
                throw new AllZeroException("Product and argument do not have the same sign");
            else
                result.Point = Product / B;
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="AAverageConditional(double, Gamma)"]/*'/>
        public static GammaPower AAverageConditional(double product, Gamma B)
        {
            if (B.IsPointMass) return GammaPower.PointMass(B.Point / product, -1);
            // b' = ab, db' = a db
            // int delta(y - ab) Ga(b; b_s, b_r) db
            // = int delta(y - b') Ga(b'/a; b_s, b_r)/a db'
            // = Ga(y/a; b_s, b_r)/a
            // =propto a^(-b_s) exp(-yb_r/a)
            return GammaPower.FromShapeAndRate(B.Shape - 1, product * B.Rate, -1);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="BAverageConditional(double, Gamma)"]/*'/>
        public static GammaPower BAverageConditional(double product, Gamma A)
        {
            return AAverageConditional(product, A);
        }

#if false
        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="AAverageConditional(double, Gamma, Gamma)"]/*'/>
        public static Gamma AAverageConditional(double Product, Gamma A, [SkipIfUniform] Gamma B)
        {
            // Z = int_x int_y delta(c - xy) Ga(x;ax,bx) Ga(y;ay,by) dx dy
            //   = int_x Ga(x;ax,bx) Ga(c/x;ay,by)/x dx
            //   = bx^ax by^ay /Gamma(ax)/Gamma(ay) 2 (bx/by)^(-(ax-ay)/2) BesselK(ax-ay, 2 sqrt(bx by))
            throw new NotImplementedException(); // BesselK is not implemented yet
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="BAverageConditional(double, Gamma, Gamma)"]/*'/>
        public static Gamma BAverageConditional(double Product, [SkipIfUniform] Gamma A, Gamma B)
        {
            return AAverageConditional(Product, B, A);
        }
#endif

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="BAverageConditional(Gamma, double)"]/*'/>
        public static Gamma BAverageConditional([SkipIfUniform] Gamma Product, double A)
        {
            return AAverageConditional(Product, A);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="BAverageConditional(double, double)"]/*'/>
        public static Gamma BAverageConditional(double Product, double A)
        {
            return AAverageConditional(Product, A);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="LogAverageFactor(Gamma, Gamma, double)"]/*'/>
        public static double LogAverageFactor(Gamma product, Gamma a, double b)
        {
            Gamma to_product = GammaProductOp.ProductAverageConditional(a, b);
            return to_product.GetLogAverageOf(product);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="LogAverageFactor(Gamma, double, Gamma)"]/*'/>
        public static double LogAverageFactor(Gamma product, double a, Gamma b)
        {
            return LogAverageFactor(product, b, a);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="LogAverageFactor(double, Gamma, double)"]/*'/>
        public static double LogAverageFactor(double product, Gamma a, double b)
        {
            if (b == 0) return (product == 0) ? 0.0 : double.NegativeInfinity;
            Gamma to_product = GammaProductOp.ProductAverageConditional(a, b);
            return to_product.GetLogProb(product);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="LogAverageFactor(double, double, Gamma)"]/*'/>
        public static double LogAverageFactor(double product, double a, Gamma b)
        {
            return LogAverageFactor(product, b, a);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="LogEvidenceRatio(Gamma, Gamma, double)"]/*'/>
        [Skip]
        public static double LogEvidenceRatio(Gamma product, Gamma a, double b)
        {
            return 0.0;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="LogEvidenceRatio(Gamma, double, Gamma)"]/*'/>
        [Skip]
        public static double LogEvidenceRatio(Gamma product, double a, Gamma b)
        {
            return LogEvidenceRatio(product, b, a);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="LogEvidenceRatio(double, Gamma, double)"]/*'/>
        public static double LogEvidenceRatio(double product, Gamma a, double b)
        {
            return LogAverageFactor(product, a, b);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp"]/message_doc[@name="LogEvidenceRatio(double, double, Gamma)"]/*'/>
        public static double LogEvidenceRatio(double product, double a, Gamma b)
        {
            return LogEvidenceRatio(product, b, a);
        }
    }

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "Ratio", typeof(double), typeof(double))]
    [Quality(QualityBand.Preview)]
    public static class GammaRatioOp
    {
        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="RatioAverageConditional(Gamma, double)"]/*'/>
        public static Gamma RatioAverageConditional([SkipIfUniform] Gamma A, double B)
        {
            if (A.IsPointMass)
                return Gamma.PointMass(A.Point / B);
            if (B == 0)
                return Gamma.PointMass(double.PositiveInfinity);
            return GammaProductOp.AAverageConditional(A, B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="AAverageConditional(double, double)"]/*'/>
        public static Gamma AAverageConditional(double ratio, double B)
        {
            if (B == 0)
            {
                if (ratio != double.PositiveInfinity)
                    throw new AllZeroException();
                return Gamma.Uniform();
            }
            return Gamma.PointMass(ratio * B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="AAverageConditional(Gamma, double)"]/*'/>
        public static Gamma AAverageConditional([SkipIfUniform] Gamma ratio, double B)
        {
            if (ratio.IsPointMass)
                return AAverageConditional(ratio.Point, B);
            if (B == 0)
                return Gamma.Uniform();
            // Ga(a/b; y_s, y_r) =propto a^(y_s-1) exp(-a y_r/b)
            return Gamma.FromShapeAndRate(ratio.Shape, ratio.Rate / B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="AAverageConditional(double, Gamma)"]/*'/>
        public static Gamma AAverageConditional(double ratio, Gamma B)
        {
            if (B.IsPointMass)
                return AAverageConditional(ratio, B.Point);
            // a' = a/b, da' = -a/b^2 db = -(a')^2/a
            // int delta(y - a/b) Ga(b; b_s, b_r) db
            // = int delta(y - a') Ga(a/a'; b_s, b_r) a/(a')^2 da'
            // = Ga(a/y; b_s, b_r) a/y^2
            // =propto a^(b_s+1) exp(-a/y b_r)
            return Gamma.FromShapeAndRate(B.Shape + 1, B.Rate / ratio);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="RatioAverageConditional(double, Gamma)"]/*'/>
        public static GammaPower RatioAverageConditional(double A, Gamma B)
        {
            if (B.IsPointMass)
                return GammaPower.PointMass(A / B.Point, -1);
            return GammaPower.FromShapeAndRate(B.Shape, B.Rate * A, -1);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="BAverageConditional(Gamma, double)"]/*'/>
        public static GammaPower BAverageConditional([SkipIfUniform] Gamma ratio, double A)
        {
            if (ratio.IsPointMass)
                return GammaPower.PointMass(BAverageConditional(ratio.Point, A).Point, -1);
            // Ga(a/b; y_s, y_r) =propto b^(-y_s+1) exp(-a/b y_r)
            return GammaPower.FromShapeAndRate(ratio.Shape - 2, ratio.Rate * A, -1);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="BAverageConditional(double, double)"]/*'/>
        public static Gamma BAverageConditional(double ratio, double A)
        {
            return GammaProductOp.AAverageConditional(A, ratio);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="BAverageConditional(double, Gamma)"]/*'/>
        public static Gamma BAverageConditional(double ratio, Gamma A)
        {
            if (A.IsPointMass)
                return BAverageConditional(ratio, A);
            // a' = a/b,  da' = da/b
            // int delta(y - a/b) Ga(a; a_s, a_r) da
            // = int b delta(y - a') Ga(b a'; a_s, a_r) da'
            // = b Ga(b y; a_s, a_r) 
            // b Ga(b y; a_s, a_r) = b^a_s y^(a_s-1) exp(-by a_r) a_r^(a_s)/Gamma(a_s)
            // = Ga(b; a_s+1, y a_r) a_s / a_r / y^2
            return Gamma.FromNatural(A.Shape, A.Rate * ratio);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="LogAverageFactor(Gamma, Gamma, double)"]/*'/>
        public static double LogAverageFactor(Gamma ratio, Gamma a, double b)
        {
            Gamma to_ratio = GammaRatioOp.RatioAverageConditional(a, b);
            return to_ratio.GetLogAverageOf(ratio);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="LogAverageFactor(double, double, double)"]/*'/>
        public static double LogAverageFactor(double ratio, double A, double B)
        {
            return (B * ratio == A) ? 0.0 : double.NegativeInfinity;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="LogAverageFactor(double, double, Gamma)"]/*'/>
        public static double LogAverageFactor(double ratio, double A, Gamma B)
        {
            if (B.IsPointMass)
                return LogAverageFactor(ratio, A, B.Point);
            if (A == 0)
            {
                if (ratio == 0)
                    return 0;
                else
                    return double.NegativeInfinity;
            }
            return GammaPower.GetLogProb(ratio / A, B.Shape, B.Rate, -1);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="LogAverageFactor(double, Gamma, Gamma)"]/*'/>
        public static double LogAverageFactor(double ratio, Gamma A, Gamma B)
        {
            if (B.IsPointMass)
                return LogAverageFactor(ratio, A, B.Point);
            if (A.IsPointMass)
                return LogAverageFactor(ratio, A.Point, B);
            // a' = a/b,  da' = da/b
            // int delta(y - a/b) Ga(a; a_s, a_r) p(b) da db
            // = int b delta(y - a') Ga(b a'; a_s, a_r) p(b) da' db
            // = int b Ga(b y; a_s, a_r) p(b) db
            // b Ga(b y; a_s, a_r) = b^a_s y^(a_s-1) exp(-by a_r) a_r^(a_s)/Gamma(a_s)
            // = Ga(b; a_s+1, y a_r) a_s / a_r / y^2
            Gamma like = Gamma.FromNatural(A.Shape, A.Rate * ratio);
            return B.GetLogAverageOf(like) + Math.Log(A.GetMean() / (ratio * ratio));
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="LogAverageFactor(double, Gamma, double)"]/*'/>
        public static double LogAverageFactor(double ratio, Gamma a, double b)
        {
            Gamma to_ratio = GammaRatioOp.RatioAverageConditional(a, b);
            return to_ratio.GetLogProb(ratio);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="LogEvidenceRatio(Gamma, Gamma, double)"]/*'/>
        [Skip]
        public static double LogEvidenceRatio(Gamma ratio, Gamma a, double b)
        {
            return 0.0;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="LogEvidenceRatio(double, Gamma, double)"]/*'/>
        public static double LogEvidenceRatio(double ratio, Gamma a, double b)
        {
            return LogAverageFactor(ratio, a, b);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp"]/message_doc[@name="LogEvidenceRatio(double, double, Gamma)"]/*'/>
        public static double LogEvidenceRatio(double ratio, double a, Gamma b)
        {
            return LogAverageFactor(ratio, a, b);
        }
    }

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp_Laplace"]/doc/*'/>
    [FactorMethod(typeof(Factor), "Ratio", typeof(double), typeof(double))]
    [Buffers("Q")]
    [Quality(QualityBand.Experimental)]
    public static class GammaRatioOp_Laplace
    {
        // derivatives of the factor marginalized over Ratio and A
        private static double[] dlogfs(double b, Gamma ratio, Gamma A)
        {
            if (ratio.IsPointMass)
            {
                // int delta(a/b - y) Ga(a; s, r) da = int b delta(a' - y) Ga(a'b; s, r) da' = b Ga(y*b; s, r)
                // logf = s*log(b) - y*r*b
                double p = 1 / b;
                double p2 = p * p;
                double shape = A.Shape;
                double dlogf = shape * p - ratio.Point * A.Rate;
                double ddlogf = -shape * p2;
                double dddlogf = 2 * shape * p * p2;
                double d4logf = -6 * shape * p2 * p2;
                return new double[] { dlogf, ddlogf, dddlogf, d4logf };
            }
            else if (A.IsPointMass)
            {
                // Ga(a/b; y_a, y_b)
                // logf = (1-y_a)*log(b) - y_b*a/b
                double p = 1 / b;
                double p2 = p * p;
                double c = ratio.Rate * A.Point;
                double shape = 1 - ratio.Shape;
                double dlogf = shape * p + c * p2;
                double ddlogf = -shape * p2 - 2 * c * p * p2;
                double dddlogf = 2 * shape * p * p2 + 6 * c * p2 * p2;
                double d4logf = -6 * shape * p2 * p2 - 24 * c * p2 * p2 * p;
                return new double[] { dlogf, ddlogf, dddlogf, d4logf };
            }
            else
            {
                // int Ga(a/b; y_s, y_r) Ga(a; s, r) da = y_r^(y_s) r^s b^s / (br + y_r)^(y_s + s-1) 
                // logf = s*log(b) - (s+y_s-1)*log(b*r + y_r)
                double r = A.Rate;
                double r2 = r * r;
                double p = 1 / (b * r + ratio.Rate);
                double p2 = p * p;
                double b2 = b * b;
                double shape = A.Shape;
                double shape2 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(ratio.Shape, shape);
                double dlogf = shape / b - shape2 * p;
                double ddlogf = -shape / b2 + shape2 * p2 * r;
                double dddlogf = 2 * shape / (b * b2) - 2 * shape2 * p * p2 * r2;
                double d4logf = -6 * shape / (b2 * b2) + 6 * shape2 * p2 * p2 * r * r2;
                return new double[] { dlogf, ddlogf, dddlogf, d4logf };
            }
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp_Laplace"]/message_doc[@name="QInit()"]/*'/>
        [Skip]
        public static Gamma QInit()
        {
            return Gamma.Uniform();
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp_Laplace"]/message_doc[@name="Q(Gamma, Gamma, Gamma)"]/*'/>
        [Fresh]
        public static Gamma Q(Gamma ratio, [Proper] Gamma A, [Proper] Gamma B)
        {
            if (B.IsPointMass)
                return B;
            if (ratio.IsPointMass)
                return GammaRatioOp.BAverageConditional(ratio.Point, A) * B;
            double shape1 = A.Shape + B.Shape;
            double shape2 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(A.Shape, ratio.Shape);
            // find the maximum of the factor marginalized over Ratio and A, times B
            // logf = s*log(b) - (s+ya-1)*log(b*r + yb)
            // let b' = b*r and maximize over b'
            double x = GammaFromShapeAndRateOp_Slow.FindMaximum(shape1, shape2, ratio.Rate, B.Rate / A.Rate);
            if (x == 0)
                return B;
            x /= A.Rate;
            double[] dlogfss = dlogfs(x, ratio, A);
            double dlogf = dlogfss[0];
            double ddlogf = dlogfss[1];
            return GammaFromShapeAndRateOp_Laplace.GammaFromDerivatives(B, x, dlogf, ddlogf);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp_Laplace"]/message_doc[@name="LogAverageFactor(Gamma, Gamma, Gamma, Gamma)"]/*'/>
        public static double LogAverageFactor(Gamma ratio, Gamma A, Gamma B, Gamma q)
        {
            if (B.IsPointMass)
                return GammaRatioOp.LogAverageFactor(ratio, A, B.Point);
            if (A.IsPointMass)
            {
                // int Ga(a/b; y_s, y_r) Ga(b; s, r) db
                // = int a^(y_s-1) b^(-y_s+1) exp(-a/b y_r) y_r^(y_s)/Gamma(y_s) Ga(b; s, r) db
                // = a^(y_s-1) y_r^(y_s)/Gamma(y_s)/Gamma(s) r^s int b^(s-y_s) exp(-a/b y_r -rb) db
                // this requires BesselK
                throw new NotImplementedException();
            }
            if (ratio.IsPointMass)
            {
                return GammaRatioOp.LogAverageFactor(ratio.Point, A, B);
            }
            // int Ga(a/b; y_s, y_r) Ga(a; s, r) da = b^s / (br + y_r)^(y_s + s-1)  Gamma(y_s+s-1)
            double x = q.GetMean();
            double shape = A.Shape;
            double shape2 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(ratio.Shape, shape);
            double logf = shape * Math.Log(x) - shape2 * Math.Log(x * A.Rate + ratio.Rate) +
              MMath.GammaLn(shape2) - A.GetLogNormalizer() - ratio.GetLogNormalizer();
            double logz = logf + B.GetLogProb(x) - q.GetLogProb(x);
            return logz;
        }

        private static double LogAverageFactor_slow(Gamma ratio, Gamma A, [Proper] Gamma B)
        {
            return LogAverageFactor(ratio, A, B, Q(ratio, A, B));
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp_Laplace"]/message_doc[@name="LogEvidenceRatio(Gamma, Gamma, Gamma, Gamma, Gamma)"]/*'/>
        public static double LogEvidenceRatio(Gamma ratio, Gamma A, Gamma B, Gamma to_ratio, Gamma q)
        {
            return LogAverageFactor(ratio, A, B, q) - to_ratio.GetLogAverageOf(ratio);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp_Laplace"]/message_doc[@name="LogEvidenceRatio(double, Gamma, Gamma, Gamma)"]/*'/>
        public static double LogEvidenceRatio(double ratio, Gamma A, Gamma B, Gamma q)
        {
            return LogAverageFactor(Gamma.PointMass(ratio), A, B, q);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp_Laplace"]/message_doc[@name="BAverageConditional(Gamma, Gamma, Gamma, Gamma)"]/*'/>
        public static Gamma BAverageConditional([SkipIfUniform] Gamma ratio, [Proper] Gamma A, [Proper] Gamma B, Gamma q)
        {
            if (ratio.IsPointMass)
                // AAverageConditional computes (1st arg)/(2nd arg)
                return GammaRatioOp.BAverageConditional(ratio.Point, A);
            if (B.IsPointMass)
                throw new NotImplementedException();
            if (q.IsUniform())
                q = Q(ratio, A, B);
            double x = q.GetMean();
            double[] g = new double[] { x, 1, 0, 0 };
            double bMean, bVariance;
            GaussianOp_Laplace.LaplaceMoments(q, g, dlogfs(x, ratio, A), out bMean, out bVariance);
            Gamma bMarginal = Gamma.FromMeanAndVariance(bMean, bVariance);
            Gamma result = new Gamma();
            result.SetToRatio(bMarginal, B, true);
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp_Laplace"]/message_doc[@name="RatioAverageConditional(Gamma, Gamma, Gamma)"]/*'/>
        public static Gamma RatioAverageConditional(Gamma ratio, [Proper] Gamma A, [SkipIfUniform] Gamma B)
        {
            if (B.IsPointMass)
                return GammaRatioOp.RatioAverageConditional(A, B.Point);
            if (A.IsPointMass)
                throw new NotImplementedException();
            if (ratio.Rate < 1e-20)
                ratio.Rate = 1e-20;
            // int delta(r - a/b) Ga(a; s_a, r_a) da = b Ga(rb; s_a, r_a)
            // int b Ga(rb; s_a, r_a) Ga(b; s_b, r_b) db = r^(s_a-1) / (r*r_a + r_b)^(s_a + s_b)
            Gamma A2 = Gamma.FromShapeAndRate(A.Shape - 1, A.Rate);
            Gamma B2 = Gamma.FromShapeAndRate(B.Shape + 2, B.Rate);
            Gamma q = Q(B2, A2, ratio);
            return BAverageConditional(B2, A2, ratio, q);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioOp_Laplace"]/message_doc[@name="AAverageConditional(Gamma, Gamma, Gamma, Gamma)"]/*'/>
        public static Gamma AAverageConditional(Gamma ratio, Gamma A, [SkipIfUniform] Gamma B, Gamma q)
        {
            if (ratio.IsPointMass)
                return GammaRatioOp.AAverageConditional(ratio.Point, B);
            if (B.IsPointMass)
                return GammaRatioOp.AAverageConditional(ratio, B.Point);
            if (A.IsPointMass)
                throw new NotImplementedException();

            double aMean, aVariance;
            double x = q.GetMean();
            double x2 = x * x;
            double p = 1 / (ratio.Rate + A.Rate * x);
            double p2 = p * p;
            double shape2 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(ratio.Shape, A.Shape);
            // aMean = shape2/(y_r/b + a_r)
            // aVariance = E[shape2*(shape2+1)/(y_r/b + a_r)^2] - aMean^2 = var(shape2/(y_r/b + a_r)) + E[shape2/(y_r/b + a_r)^2]
            //           = shape2^2*var(1/(y_r/b + a_r)) + shape2*(var(1/(y_r/b + a_r)) + (aMean/shape2)^2)
            double[] g = new double[] { x * p, ratio.Rate * p2, -2 * p2 * p * ratio.Rate * A.Rate, 6 * p2 * p2 * ratio.Rate * A.Rate * A.Rate };
            double pMean, pVariance;
            GaussianOp_Laplace.LaplaceMoments(q, g, dlogfs(x, ratio, A), out pMean, out pVariance);
            aMean = shape2 * pMean;
            aVariance = shape2 * shape2 * pVariance + shape2 * (pVariance + pMean * pMean);

            Gamma aMarginal = Gamma.FromMeanAndVariance(aMean, aVariance);
            Gamma result = new Gamma();
            result.SetToRatio(aMarginal, A, true);
            if (double.IsNaN(result.Shape) || double.IsNaN(result.Rate))
                throw new InferRuntimeException("result is nan");
            return result;
        }
    }

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp_Laplace"]/doc/*'/>
    [FactorMethod(typeof(Factor), "Product", typeof(double), typeof(double))]
    [Buffers("Q")]
    [Quality(QualityBand.Experimental)]
    public static class GammaProductOp_Laplace
    {
        // derivatives of the factor marginalized over Product and A
        private static double[] dlogfs(double b, Gamma product, Gamma A)
        {
            if (product.IsPointMass)
            {
                // int delta(ab - y) Ga(a; s, r) da = int delta(a' - y) Ga(a'/b; s, r)/b da' = Ga(y/b; s, r)/b
                // logf = -s*log(b) - y*r/b
                double p = 1 / b;
                double p2 = p * p;
                double shape = A.Shape;
                double c = product.Point * A.Rate;
                double dlogf = -shape * p + c * p2;
                double ddlogf = shape * p2 - 2 * c * p2 * p;
                double dddlogf = -2 * shape * p * p2 + 6 * c * p2 * p2;
                double d4logf = 6 * shape * p2 * p2 - 24 * c * p2 * p2 * p;
                return new double[] { dlogf, ddlogf, dddlogf, d4logf };
            }
            else if (A.IsPointMass)
            {
                // Ga(ab; y_a, y_b)
                // logf = (y_a-1)*log(b) - y_b*ab
                double p = 1 / b;
                double p2 = p * p;
                double c = product.Rate * A.Point;
                double shape = product.Shape - 1;
                double dlogf = shape * p - c;
                double ddlogf = -shape * p2;
                double dddlogf = 2 * shape * p * p2;
                double d4logf = -6 * shape * p2 * p2;
                return new double[] { dlogf, ddlogf, dddlogf, d4logf };
            }
            else
            {
                // int Ga(ab; y_s, y_r) Ga(a; s, r) da = y_r^(y_s) r^s b^(y_s-1) / (r + b y_r)^(y_s + s-1) 
                // logf = (y_s-1)*log(b) - (s+y_s-1)*log(r + b*y_r)
                double r = product.Rate;
                double r2 = r * r;
                double p = 1 / (A.Rate + b * r);
                double p2 = p * p;
                double b2 = b * b;
                double shape = product.Shape - 1;
                double shape2 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(product.Shape, A.Shape);
                double dlogf = shape / b - shape2 * p * r;
                double ddlogf = -shape / b2 + shape2 * p2 * r2;
                double dddlogf = 2 * shape / (b * b2) - 2 * shape2 * p * p2 * r2 * r;
                double d4logf = -6 * shape / (b2 * b2) + 6 * shape2 * p2 * p2 * r2 * r2;
                return new double[] { dlogf, ddlogf, dddlogf, d4logf };
            }
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp_Laplace"]/message_doc[@name="QInit()"]/*'/>
        [Skip]
        public static Gamma QInit()
        {
            return Gamma.Uniform();
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp_Laplace"]/message_doc[@name="Q(Gamma, Gamma, Gamma)"]/*'/>
        [Fresh]
        public static Gamma Q(Gamma product, [Proper] Gamma A, [Proper] Gamma B)
        {
            if (B.IsPointMass)
                return B;
            if (A.IsPointMass)
                throw new NotImplementedException();
            double x;
            if (product.IsPointMass)
            {
                // logf = -a_s*log(b) - y*a_r/b
                // logp = b_s*log(b) - b_r*b
                // dlogfp = (b_s-a_s)/b - b_r + y*a_r/b^2 = 0
                // -b_r b^2 + (b_s-a_s) b + y*a_r = 0
                double shape = B.Shape - A.Shape;
                double y = product.Point;
                x = (Math.Sqrt(shape * shape + 4 * B.Rate * A.Rate * y) + shape) / 2 / B.Rate;
            }
            else
            {
                double shape1 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(B.Shape, product.Shape);
                double shape2 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(A.Shape, product.Shape);
                // find the maximum of the factor marginalized over Product and A, times B
                // logf = (y_s-1)*log(b) - (y_s+a_s-1)*log(b*y_r + a_r)
                // let b' = b*y_r and maximize over b'
                x = GammaFromShapeAndRateOp_Slow.FindMaximum(shape1, shape2, A.Rate, B.Rate / product.Rate);
                if (x == 0)
                    return B;
                x /= product.Rate;
            }
            double[] dlogfss = dlogfs(x, product, A);
            double dlogf = dlogfss[0];
            double ddlogf = dlogfss[1];
            return GammaFromShapeAndRateOp_Laplace.GammaFromDerivatives(B, x, dlogf, ddlogf);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp_Laplace"]/message_doc[@name="LogAverageFactor(Gamma, Gamma, Gamma, Gamma)"]/*'/>
        public static double LogAverageFactor(Gamma product, Gamma A, Gamma B, Gamma q)
        {
            if (B.IsPointMass)
                return GammaProductOp.LogAverageFactor(product, A, B.Point);
            if (A.IsPointMass)
                return GammaProductOp.LogAverageFactor(product, A.Point, B);
            double x = q.GetMean();
            double logf;
            if (product.IsPointMass)
            {
                // Ga(y/b; s, r)/b
                double y = product.Point;
                logf = (A.Shape - 1) * Math.Log(y) - A.Shape * Math.Log(x) - A.Rate * y / x - A.GetLogNormalizer();
            }
            else
            {
                // int Ga(ab; y_s, y_r) Ga(a; s, r) da = b^(y_s-1) / (r + b y_r)^(y_s + s-1)  Gamma(y_s+s-1)
                double shape = product.Shape - 1;
                double shape2 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(product.Shape, A.Shape);
                logf = shape * Math.Log(x) - shape2 * Math.Log(A.Rate + x * product.Rate) +
                  MMath.GammaLn(shape2) - A.GetLogNormalizer() - product.GetLogNormalizer();
            }
            double logz = logf + B.GetLogProb(x) - q.GetLogProb(x);
            return logz;
        }

        private static double LogAverageFactor_slow(Gamma product, Gamma A, [Proper] Gamma B)
        {
            return LogAverageFactor(product, A, B, Q(product, A, B));
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp_Laplace"]/message_doc[@name="LogEvidenceRatio(Gamma, Gamma, Gamma, Gamma, Gamma)"]/*'/>
        public static double LogEvidenceRatio(Gamma product, Gamma A, Gamma B, Gamma to_product, Gamma q)
        {
            return LogAverageFactor(product, A, B, q) - to_product.GetLogAverageOf(product);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp_Laplace"]/message_doc[@name="LogEvidenceRatio(double, Gamma, Gamma, Gamma)"]/*'/>
        public static double LogEvidenceRatio(double product, Gamma A, Gamma B, Gamma q)
        {
            return LogAverageFactor(Gamma.PointMass(product), A, B, q);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp_Laplace"]/message_doc[@name="BAverageConditional(Gamma, Gamma, Gamma, Gamma)"]/*'/>
        public static Gamma BAverageConditional([SkipIfUniform] Gamma product, [Proper] Gamma A, [Proper] Gamma B, Gamma q)
        {
            if (A.IsPointMass)
                return GammaProductOp.BAverageConditional(product, A.Point);
            if (B.IsPointMass)
                throw new NotImplementedException();
            if (q.IsUniform())
                q = Q(product, A, B);
            double x = q.GetMean();
            double[] g = new double[] { x, 1, 0, 0 };
            double bMean, bVariance;
            GaussianOp_Laplace.LaplaceMoments(q, g, dlogfs(x, product, A), out bMean, out bVariance);
            Gamma bMarginal = Gamma.FromMeanAndVariance(bMean, bVariance);
            Gamma result = new Gamma();
            result.SetToRatio(bMarginal, B, true);
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp_Laplace"]/message_doc[@name="ProductAverageConditional(Gamma, Gamma, Gamma, Gamma)"]/*'/>
        public static Gamma ProductAverageConditional(Gamma product, [Proper] Gamma A, [SkipIfUniform] Gamma B, Gamma q)
        {
            if (B.IsPointMass)
                return GammaProductOp.ProductAverageConditional(A, B.Point);
            if (A.IsPointMass)
                return GammaProductOp.ProductAverageConditional(A.Point, B);
            if (product.IsPointMass)
                throw new NotImplementedException();

            double yMean, yVariance;
            double x = q.GetMean();
            double x2 = x * x;
            double r = product.Rate;
            double r2 = r * r;
            double p = 1 / (x * r + A.Rate);
            double p2 = p * p;
            double shape2 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(product.Shape, A.Shape);
            // yMean = shape2*b/(b y_r + a_r)
            // yVariance = E[shape2*(shape2+1)*b^2/(b y_r + a_r)^2] - aMean^2 = var(shape2*b/(b y_r + a_r)) + E[shape2*b^2/(b y_r + a_r)^2]
            //           = shape2^2*var(b/(b y_r + a_r)) + shape2*(var(b/(b y_r + a_r)) + (aMean/shape2)^2)
            double[] g = new double[] { x * p, A.Rate * p2, -2 * p2 * p * A.Rate * r, 6 * p2 * p2 * A.Rate * r2 };
            double pMean, pVariance;
            GaussianOp_Laplace.LaplaceMoments(q, g, dlogfs(x, product, A), out pMean, out pVariance);
            yMean = shape2 * pMean;
            yVariance = shape2 * shape2 * pVariance + shape2 * (pVariance + pMean * pMean);

            Gamma yMarginal = Gamma.FromMeanAndVariance(yMean, yVariance);
            Gamma result = new Gamma();
            result.SetToRatio(yMarginal, product, true);
            if (double.IsNaN(result.Shape) || double.IsNaN(result.Rate))
                throw new InferRuntimeException("result is nan");
            return result;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductOp_Laplace"]/message_doc[@name="AAverageConditional(Gamma, Gamma, Gamma, Gamma)"]/*'/>
        public static Gamma AAverageConditional(Gamma product, Gamma A, [SkipIfUniform] Gamma B, Gamma q)
        {
            if (B.IsPointMass)
                return GammaProductOp.AAverageConditional(product, B.Point);
            if (A.IsPointMass)
                throw new NotImplementedException();
            double aMean, aVariance;
            double x = q.GetMean();
            if (product.IsPointMass)
            {
                // Z = int Ga(y/b; s, r)/b Ga(b; b_s, b_r) db
                // E[a] = E[y/b]
                // E[a^2] = E[y^2/b^2]
                // aVariance = E[a^2] - aMean^2
                double y = product.Point;
                double p = 1 / x;
                double p2 = p * p;
                double[] g = new double[] { p, -p2, 2 * p2 * p, -6 * p2 * p2 };
                double pMean, pVariance;
                GaussianOp_Laplace.LaplaceMoments(q, g, dlogfs(x, product, A), out pMean, out pVariance);
                aMean = y * pMean;
                aVariance = y * y * pVariance;
            }
            else
            {
                double x2 = x * x;
                double r = product.Rate;
                double r2 = r * r;
                double p = 1 / (x * r + A.Rate);
                double p2 = p * p;
                double shape2 = GammaFromShapeAndRateOp_Slow.AddShapesMinus1(product.Shape, A.Shape);
                // aMean = shape2/(b y_r + a_r)
                // aVariance = E[shape2*(shape2+1)/(b y_r + a_r)^2] - aMean^2 = var(shape2/(b y_r + a_r)) + E[shape2/(b y_r + a_r)^2]
                //           = shape2^2*var(1/(b y_r + a_r)) + shape2*(var(1/(b y_r + a_r)) + (aMean/shape2)^2)
                double[] g = new double[] { p, -r * p2, 2 * p2 * p * r2, -6 * p2 * p2 * r2 * r };
                double pMean, pVariance;
                GaussianOp_Laplace.LaplaceMoments(q, g, dlogfs(x, product, A), out pMean, out pVariance);
                aMean = shape2 * pMean;
                aVariance = shape2 * shape2 * pVariance + shape2 * (pVariance + pMean * pMean);
            }
            Gamma aMarginal = Gamma.FromMeanAndVariance(aMean, aVariance);
            Gamma result = new Gamma();
            result.SetToRatio(aMarginal, A, true);
            if (double.IsNaN(result.Shape) || double.IsNaN(result.Rate))
                throw new InferRuntimeException("result is nan");
            return result;
        }
    }

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "Product", typeof(double), typeof(double))]
    [Quality(QualityBand.Preview)]
    public static class GammaProductVmpOp
    {
        public static double AverageLogFactor(double product, Gamma a, double b)
        {
            return (b == 0) ? 0.0 : -Math.Log(b);
        }

        public static double AverageLogFactor(double product, double a, Gamma b)
        {
            return (a == 0) ? 0.0 : -Math.Log(a);
        }

        [Skip]
        public static double AverageLogFactor(Gamma product)
        {
            return 0.0;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="ProductAverageLogarithm(Gamma, Gamma)"]/*'/>
        public static Gamma ProductAverageLogarithm([SkipIfUniform] Gamma A, [SkipIfUniform] Gamma B)
        {
            // E[x] = E[a]*E[b]
            // E[log(x)] = E[log(a)]+E[log(b)]
            return Gamma.FromMeanAndMeanLog(A.GetMean() * B.GetMean(), A.GetMeanLog() + B.GetMeanLog());
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="ProductAverageLogarithm(double, Gamma)"]/*'/>
        public static Gamma ProductAverageLogarithm(double A, [SkipIfUniform] Gamma B)
        {
            if (B.IsPointMass)
                return Gamma.PointMass(A * B.Point);
            return Gamma.FromShapeAndRate(B.Shape, B.Rate / A);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="ProductAverageLogarithm(Gamma, double)"]/*'/>
        public static Gamma ProductAverageLogarithm([SkipIfUniform] Gamma A, double B)
        {
            return ProductAverageLogarithm(B, A);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="AAverageLogarithm(Gamma, Gamma)"]/*'/>
        public static Gamma AAverageLogarithm([SkipIfUniform] Gamma Product, [Proper] Gamma B)
        {
            if (B.IsPointMass)
                return AAverageLogarithm(Product, B.Point);
            if (Product.IsPointMass)
                return AAverageLogarithm(Product.Point, B);
            // (ab)^(shape-1) exp(-rate*(ab))
            return Gamma.FromShapeAndRate(Product.Shape, Product.Rate * B.GetMean());
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="AAverageLogarithm(double, Gamma)"]/*'/>
        [NotSupported(GaussianProductVmpOp.NotSupportedMessage)]
        public static Gamma AAverageLogarithm(double Product, [Proper] Gamma B)
        {
            throw new NotSupportedException(GaussianProductVmpOp.NotSupportedMessage);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="AAverageLogarithm(Gamma, double)"]/*'/>
        public static Gamma AAverageLogarithm([SkipIfUniform] Gamma Product, double B)
        {
            return GammaProductOp.AAverageConditional(Product, B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="AAverageLogarithm(double, double)"]/*'/>
        public static Gamma AAverageLogarithm(double Product, double B)
        {
            return GammaProductOp.AAverageConditional(Product, B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="BAverageLogarithm(Gamma, Gamma)"]/*'/>
        public static Gamma BAverageLogarithm([SkipIfUniform] Gamma Product, [Proper] Gamma A)
        {
            return AAverageLogarithm(Product, A);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="BAverageLogarithm(double, Gamma)"]/*'/>
        [NotSupported(GaussianProductVmpOp.NotSupportedMessage)]
        public static Gamma BAverageLogarithm(double Product, [Proper] Gamma A)
        {
            return AAverageLogarithm(Product, A);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="BAverageLogarithm(Gamma, double)"]/*'/>
        public static Gamma BAverageLogarithm([SkipIfUniform] Gamma Product, double A)
        {
            return AAverageLogarithm(Product, A);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaProductVmpOp"]/message_doc[@name="BAverageLogarithm(double, double)"]/*'/>
        public static Gamma BAverageLogarithm(double Product, double A)
        {
            return AAverageLogarithm(Product, A);
        }
    }

    /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioVmpOp"]/doc/*'/>
    [FactorMethod(typeof(Factor), "Ratio", typeof(double), typeof(double))]
    [Quality(QualityBand.Preview)]
    public static class GammaRatioVmpOp
    {
        public static double AverageLogFactor(double ratio, Gamma a, double b)
        {
            return (b == 0) ? 0.0 : Math.Log(b);
        }

        public static double AverageLogFactor(double ratio, double a, Gamma b)
        {
            return (a == 0) ? 0.0 : Math.Log(a);
        }

        [Skip]
        public static double AverageLogFactor(Gamma ratio)
        {
            return 0.0;
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioVmpOp"]/message_doc[@name="RatioAverageLogarithm(Gamma, Gamma)"]/*'/>
        public static Gamma RatioAverageLogarithm([SkipIfUniform] Gamma A, [Proper] Gamma B)
        {
            if (A.IsUniform())
                return A;
            double mean = A.GetMean() * B.GetMeanInverse();
            double meanLog = A.GetMeanLog() - B.GetMeanLog();
            return Gamma.FromMeanAndMeanLog(mean, meanLog);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioVmpOp"]/message_doc[@name="RatioAverageLogarithm(Gamma, double)"]/*'/>
        public static Gamma RatioAverageLogarithm([SkipIfUniform] Gamma A, double B)
        {
            return GammaRatioOp.RatioAverageConditional(A, B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioVmpOp"]/message_doc[@name="AAverageLogarithm(Gamma, Gamma)"]/*'/>
        public static Gamma AAverageLogarithm([SkipIfUniform] Gamma ratio, [Proper] Gamma B)
        {
            // factor is delta(r - a/b)
            // message to A is:  E[log Ga(a/b; ra, rb)] = E[(ra-1)*log(a/b) - rb*(a/b)]
            return Gamma.FromShapeAndRate(ratio.Shape, ratio.Rate * B.GetMeanInverse());
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioVmpOp"]/message_doc[@name="AAverageLogarithm(Gamma, double)"]/*'/>
        public static Gamma AAverageLogarithm([SkipIfUniform] Gamma ratio, double B)
        {
            return GammaRatioOp.AAverageConditional(ratio, B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioVmpOp"]/message_doc[@name="AAverageLogarithm(double, double)"]/*'/>
        public static Gamma AAverageLogarithm(double ratio, double B)
        {
            return GammaRatioOp.AAverageConditional(ratio, B);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioVmpOp"]/message_doc[@name="BAverageLogarithm(Gamma, Gamma, Gamma)"]/*'/>
        public static Gamma BAverageLogarithm([SkipIfUniform] Gamma ratio, [Proper] Gamma A, [Proper] Gamma B)
        {
            // using Winn's rule, message to B is: E[log Ga(a/b; ra, rb)] = E[(ra-1)*log(a/b) - rb*(a/b)]
            // which is not conjugate.  so we apply non-conjugate VMP.
            double meanInv = B.GetMeanInverse();
            double rs1 = ratio.Shape - 1;
            double s1 = B.Shape - 1;
            double mA = A.GetMean();
            double tri = MMath.Trigamma(B.Shape);
            double c = ratio.Rate * mA * meanInv;
            double dSa = -rs1 * tri + c / s1;
            double dSbb = rs1 - c;
            double denom = tri * B.Shape - 1;
            return Gamma.FromShapeAndRate(1 + (dSa * B.Shape + dSbb) / denom, (dSa + dSbb * tri) * B.Rate / denom);
        }

        private const string NotSupportedMessage = "Variational Message Passing does not support a Ratio factor with fixed output.";

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioVmpOp"]/message_doc[@name="AAverageLogarithm(double,Gamma)"]/*'/>
        /// <remarks><para>
        /// Variational Message Passing does not support a Ratio factor with fixed output
        /// </para></remarks>
        [NotSupported(NotSupportedMessage)]
        public static Gamma AAverageLogarithm(double ratio, Gamma B)
        {
            throw new NotSupportedException(NotSupportedMessage);
        }

        /// <include file='FactorDocs.xml' path='factor_docs/message_op_class[@name="GammaRatioVmpOp"]/message_doc[@name="BAverageLogarithm(double,Gamma)"]/*'/>
        /// <remarks><para>
        /// Variational Message Passing does not support a Ratio factor with fixed output
        /// </para></remarks>
        [NotSupported(NotSupportedMessage)]
        public static Gamma BAverageLogarithm(double ratio, Gamma A)
        {
            throw new NotSupportedException(NotSupportedMessage);
        }
    }
}
