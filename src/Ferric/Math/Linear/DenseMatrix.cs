﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ferric.Math.Linear
{
    public class DenseMatrix<T> : BaseMatrix<T>
    {
        T[,] data;

        public override T this[int row, int col]
        {
            get { return data[row, col]; }
            set { data[row, col] = value; }
        }

        public DenseMatrix(int rows, int cols)
        {
            this.Rows = rows;
            this.Cols = cols;
            data = new T[rows, cols];
        }

        public DenseMatrix(int rows, int cols, T[,] data, bool copy = false)
        {
            this.Rows = rows;
            this.Cols = cols;

            if (copy)
            {
                this.data = new T[rows, cols];
                for (int i = 0; i < this.Rows; ++i)
                {
                    for (int j = 0; j < this.Cols; ++j)
                    {
                        this.data[i, j] = data[i, j];
                    }
                }
            }
            else
            {
                this.data = data;
            }
        }

        #region BaseMatrix Members

        public override BaseMatrix<T> Transpose()
        {
            var res = new DenseMatrix<T>(this.Cols, this.Rows);
            for (var i = 0; i < this.Rows; ++i)
            {
                for (var j = 0; j < this.Cols; ++j)
                {
                    res[j, i] = this[i, j];
                }
            }
            return res;
        }

        public override BaseMatrix<T> ScalarMultiply(T n, bool inPlace)
        {
            var res = inPlace ? this : new DenseMatrix<T>(this.Rows, this.Cols, this.data, copy: true);

            if (typeof(T) == typeof(double))
            {
                var a = res as BaseMatrix<double>;
                var nd = Convert.ToDouble(n);

                for (var i = 0; i < a.Rows; ++i)
                {
                    for (var j = 0; j < a.Cols; ++j)
                    {
                        a[i, j] *= nd;
                    }
                }
            }
            else if (typeof(T) == typeof(int))
            {
                var a = res as BaseMatrix<int>;
                var ni = Convert.ToInt32(n);

                for (var i = 0; i < a.Rows; ++i)
                {
                    for (var j = 0; j < a.Cols; ++j)
                    {
                        a[i, j] *= ni;
                    }
                }
            }
            else 
            {
                var mul = typeof(T).GetMethod("op_Multiply", BindingFlags.Static | BindingFlags.Public);
                if (mul == null)
                    throw new ArgumentException("Unable to find a multiplication operator for " + typeof(T).FullName);

                object[] parms = new object[2];
                parms[1] = n;
                for (var i = 0; i < res.Rows; ++i)
                {
                    for (var j = 0; j < res.Cols; ++j)
                    {
                        parms[0] = res[i, j];
                        res[i, j] = (T)mul.Invoke(null, parms);
                    }
                }
            }
            return res;
        }

        public override BaseMatrix<T> Add(Matrix<T> m, bool inPlace = false)
        {
            var res = inPlace ? this : new DenseMatrix<T>(this.Rows, this.Cols, this.data, copy: true);

            if (this.Rows != m.Rows || this.Cols != m.Cols)
                throw new ArgumentException("Unable to add matrices of different dimensions");

            if (typeof(T) == typeof(double))
            {
                var a = res as BaseMatrix<double>;
                var b = m as BaseMatrix<double>;

                for (var i = 0; i < a.Rows; ++i)
                {
                    for (var j = 0; j < a.Cols; ++j)
                    {
                        a[i, j] += b[i, j];
                    }
                }
            }
            else if (typeof(T) == typeof(int))
            {
                var a = res as BaseMatrix<int>;
                var b = m as BaseMatrix<int>;

                for (var i = 0; i < a.Rows; ++i)
                {
                    for (var j = 0; j < a.Cols; ++j)
                    {
                        a[i, j] += b[i, j];
                    }
                }
            }
            else 
            {
                var add = typeof(T).GetMethod("op_Addition", BindingFlags.Static | BindingFlags.Public);
                if (add == null)
                    throw new ArgumentException("Unable to find an addition operator for " + typeof(T).FullName);

                object[] parms = new object[2];
                for (var i = 0; i < res.Rows; ++i)
                {
                    for (var j = 0; j < res.Cols; ++j)
                    {
                        parms[0] = res[i, j];
                        parms[1] = m[i, j];
                        res[i, j] = (T)add.Invoke(null, parms);
                    }
                }

            }

            return res;
        }

        public override BaseMatrix<T> Negate(bool inPlace = false)
        {
            var res = inPlace ? this : new DenseMatrix<T>(this.Rows, this.Cols, this.data, copy: true);

            if (typeof(T) == typeof(double))
            {
                var a = res as BaseMatrix<double>;
                for (var i = 0; i < a.Rows; ++i)
                {
                    for (var j = 0; j < a.Cols; ++j)
                    {
                        a[i, j] = -a[i, j];
                    }
                }
            }
            else if (typeof(T) == typeof(int))
            {
                var a = res as BaseMatrix<int>;
                for (var i = 0; i < a.Rows; ++i)
                {
                    for (var j = 0; j < a.Cols; ++j)
                    {
                        a[i, j] = -a[i, j];
                    }
                }
            }
            else 
            {
                var mul = typeof(T).GetMethod("op_UnaryNegation", BindingFlags.Static | BindingFlags.Public);
                if (mul == null)
                    throw new ArgumentException("Unable to find a unary negation operator for " + typeof(T).FullName);

                object[] parms = new object[1];
                for (var i = 0; i < res.Rows; ++i)
                {
                    for (var j = 0; j < res.Cols; ++j)
                    {
                        parms[0] = res[i, j];
                        res[i, j] = (T)mul.Invoke(null, parms);
                    }
                }
            }
            return res;
        }

        public override BaseMatrix<T> Subtract(Matrix<T> m, bool inPlace = false)
        {
            var res = inPlace ? this : new DenseMatrix<T>(this.Rows, this.Cols, this.data, copy: true);

            if (this.Rows != m.Rows || this.Cols != m.Cols)
                throw new ArgumentException("Unable to subtract matrices of different dimensions");

            if (typeof(T) == typeof(double))
            {
                var a = res as BaseMatrix<double>;
                var b = m as BaseMatrix<double>;

                for (var i = 0; i < a.Rows; ++i)
                {
                    for (var j = 0; j < a.Cols; ++j)
                    {
                        a[i, j] -= b[i, j];
                    }
                }
            }
            else if (typeof(T) == typeof(int))
            {
                var a = res as BaseMatrix<int>;
                var b = m as BaseMatrix<int>;

                for (var i = 0; i < a.Rows; ++i)
                {
                    for (var j = 0; j < a.Cols; ++j)
                    {
                        a[i, j] -= b[i, j];
                    }
                }
            }
            else 
            {
                var add = typeof(T).GetMethod("op_Subtraction", BindingFlags.Static | BindingFlags.Public);
                if (add == null)
                    throw new ArgumentException("Unable to find a subtraction operator for " + typeof(T).FullName);

                object[] parms = new object[2];
                for (var i = 0; i < res.Rows; ++i)
                {
                    for (var j = 0; j < res.Cols; ++j)
                    {
                        parms[0] = res[i, j];
                        parms[1] = m[i, j];
                        res[i, j] = (T)add.Invoke(null, parms);
                    }
                }

            }

            return res;
        }

        public override BaseMatrix<T> Multiply(Matrix<T> m, bool inPlace = false)
        {
            if (this.Cols != m.Rows)
                throw new ArgumentException("Unable to multiply nonconformable matrices");
            if (inPlace && (this.Rows != m.Rows || this.Cols != m.Cols))
                throw new ArgumentException("Unable to multiply differently-sized matrices in-place");

            var res = inPlace ? this : new DenseMatrix<T>(this.Rows, m.Cols, this.data, copy: true);

            if (typeof(T) == typeof(double))
            {
                var a = this as BaseMatrix<double>;
                var b = m as BaseMatrix<double>;
                var c = res as BaseMatrix<double>;

                for (var i = 0; i < c.Rows; ++i)
                {
                    for (var j = 0; j < c.Cols; ++j)
                    {
                        double sum = 0;
                        for (var k = 0; k < this.Cols; ++k)
                        {
                            sum += a[i, k] * b[k, j];
                        }
                        c[i, j] = sum;
                    }
                }
            }
            else if (typeof(T) == typeof(int))
            {
                var a = this as BaseMatrix<int>;
                var b = m as BaseMatrix<int>;
                var c = res as BaseMatrix<int>;

                for (var i = 0; i < c.Rows; ++i)
                {
                    for (var j = 0; j < c.Cols; ++j)
                    {
                        int sum = 1;
                        for (var k = 0; k < this.Cols; ++k)
                        {
                            sum += a[i, k] * b[k, j];
                        }
                        c[i, j] = sum;
                    }
                }
            }
            else 
            {
                var add = typeof(T).GetMethod("op_Subtraction", BindingFlags.Static | BindingFlags.Public);
                if (add == null) throw new ArgumentException("Unable to find a subtraction operator for " + typeof(T).FullName);
                var mul = typeof(T).GetMethod("op_Multiply", BindingFlags.Static | BindingFlags.Public);
                if (mul == null) throw new ArgumentException("Unable to find a multiplication operator for " + typeof(T).FullName);

                var a = this;
                var b = m;
                var c = res;

                var p = new object[2];
                for (var i = 0; i < c.Rows; ++i)
                {
                    for (var j = 0; j < c.Cols; ++j)
                    {
                        object sum = null;
                        for (var k = 0; k < this.Cols; ++k)
                        {
                            p[0] = a[i, k];
                            p[1] = b[k, j];
                            object prod = mul.Invoke(null, p);

                            if (sum == null)
                            {
                                sum = prod;
                            }
                            else
                            {
                                p[0] = sum;
                                p[1] = prod;
                                sum = add.Invoke(null, p);
                            }
                        }
                        c[i, j] = (T)sum;
                    }
                }
            }

            return res;
        }

        public override BaseMatrix<T> Inverse()
        {
            var m = this as DenseMatrix<double>;
            if (m == null)
                throw new ArgumentException("Unable to invert a non-double matrix");

            if (this.Rows != this.Cols)
                throw new ArgumentException("Unable to invert a non-square matrix");

            // inversion algo taken from http://msdn.microsoft.com/en-us/magazine/jj863137.aspx

            var n = m.Rows;
            var res = new DenseMatrix<double>(m.Rows, m.Cols, m.data, copy: true);

            int[] perm;
            int toggle;
            var lum = Decompose(res, out perm, out toggle);
            if (lum == null)
                throw new ArgumentException("Matrix is not invertible");

            var b = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                        b[j] = 1;
                    else
                        b[j] = 0;
                }

                var x = HelperSolve(lum, b);

                for (int j = 0; j < n; ++j)
                {
                    res[j, i] = x[j];
                }
            }

            return res as BaseMatrix<T>;
        }

        static DenseMatrix<double> Decompose(DenseMatrix<double> m, out int[] perm, out int toggle)
        {
            if (m.Rows != m.Cols)
                throw new ArgumentException("Unable to decompose a non-square matrix");

            int n = m.Rows;
            var result = new DenseMatrix<double>(m.Rows, m.Cols, m.data, copy: true);

            perm = new int[n];
            for (int i = 0; i < n; ++i) { perm[i] = i; }

            toggle = 1;

            for (int j = 0; j < n - 1; ++j)
            {
                double max = System.Math.Abs(result[j, j]);
                int maxRow = j;
                for (int i = j + 1; i < n; ++i)
                {
                    if (result[i, j] > max)
                    {
                        max = result[i, j];
                        maxRow = i;
                    }
                }

                if (maxRow != j)
                {
                    for (int k = 0; k < n; ++k)
                    {
                        var temp = result[maxRow, k];
                        result[maxRow, k] = result[j, k];
                        result[j, k] = temp;
                    }

                    var ptmp = perm[maxRow];
                    perm[maxRow] = perm[j];
                    perm[j] = ptmp;

                    toggle = -toggle;
                }

                if (System.Math.Abs(result[j, j]) < 1.0e-20)
                    throw new Exception("Unable to decompose a non-decomposable matrix");

                for (int i = j + 1; i < n; ++i)
                {
                    result[i, j] /= result[j, j];
                    for (int k = j + 1; k < n; ++k)
                    {
                        result[i, k] -= result[i, j] * result[j, k];
                    }
                }
            }

            return result;
        }

        static double[] HelperSolve(Matrix<double> m, double[] b)
        {
            int n = m.Rows;
            var x = new double[b.Length];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= m[i, j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= m[n - 1, n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= m[i, j] * x[j];
                x[i] = sum / m[i, i];
            }

            return x;
        }

        #endregion

        #region ISerializable Members

        public DenseMatrix(SerializationInfo info, StreamingContext context)
        {
            this.Rows = (int)info.GetValue("rows", typeof(int));
            this.Cols = (int)info.GetValue("cols", typeof(int));
            this.data = (T[,])info.GetValue("data", typeof(T[,]));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("rows", this.Rows);
            info.AddValue("cols", this.Cols);
            info.AddValue("data", this.data);
        }

        #endregion
    }

    public class DenseVector<T> : DenseMatrix<T>, Vector<T>
    {
        public int Dimensions { get { return this.Rows; } }

        public DenseVector(int dimensions)
            : base(dimensions, 1)
        {
        }

        public DenseVector(int dimensions, T[] data)
            : base(dimensions, 1)
        {
            for (var i = 0; i < dimensions; ++i)
            {
                this[i, 0] = data[i];
            }
        }
    }
}
