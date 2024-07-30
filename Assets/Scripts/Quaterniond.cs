using System;
using Unity.Mathematics;
using UnityEngine;

public struct Quaterniond
{
    #region public members

    public double x;
    public double y;
    public double z;
    public double w;

    #endregion

    #region constructor

    public Quaterniond(double p_x, double p_y, double p_z, double p_w)
    {
        x = p_x;
        y = p_y;
        z = p_z;
        w = p_w;
    }

    #endregion

    #region public properties

    public double this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return x;
                case 1:
                    return y;
                case 2:
                    return z;
                case 3:
                    return w;
                default:
                    throw new IndexOutOfRangeException("Invalid Quaterniond index!");
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                case 2:
                    z = value;
                    break;
                case 3:
                    w = value;
                    break;
                default:
                    throw new IndexOutOfRangeException("Invalid Quaterniond index!");
            }
        }
    }

    public static Quaterniond identity
    {
        get
        {
            return new Quaterniond(0, 0, 0, 1);
        }
    }

    public double3 eulerAngles
    {
        get
        {
            double4x4 m = QuaternionToMatrix(this);
            return (MatrixToEuler(m) * 180 / Math.PI);
        }
        set
        {
            this = Euler(value);
        }
    }

    #endregion

    #region public functions

    public static double Angle(Quaterniond a, Quaterniond b)
    {
        double single = Dot(a, b);
        return Math.Acos(Math.Min(Math.Abs(single), 1f)) * 2f * (180 / Math.PI);
    }

    public static Quaterniond AngleAxis(double angle, double3 axis)
    {
        axis = math.normalize(axis);
        angle = angle / 180D * Math.PI;

        Quaterniond q = new Quaterniond();

        double halfAngle = angle * 0.5D;
        double s = Math.Sin(halfAngle);

        q.w = Math.Cos(halfAngle);
        q.x = s * axis.x;
        q.y = s * axis.y;
        q.z = s * axis.z;

        return q;
    }

    public static double Dot(Quaterniond a, Quaterniond b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
    }

    public static Quaterniond Euler(double3 euler)
    {
        return Euler(euler.x, euler.y, euler.z);
    }

    public static Quaterniond Euler(double x, double y, double z)
    {
        double cX = Math.Cos(x * Math.PI / 360);
        double sX = Math.Sin(x * Math.PI / 360);

        double cY = Math.Cos(y * Math.PI / 360);
        double sY = Math.Sin(y * Math.PI / 360);

        double cZ = Math.Cos(z * Math.PI / 360);
        double sZ = Math.Sin(z * Math.PI / 360);

        Quaterniond qX = new Quaterniond(sX, 0, 0, cX);
        Quaterniond qY = new Quaterniond(0, sY, 0, cY);
        Quaterniond qZ = new Quaterniond(0, 0, sZ, cZ);

        Quaterniond q = (qY * qX) * qZ;

        return q;
    }

    public static Quaterniond FromToRotation(double3 fromDirection, double3 toDirection)
    {
        throw new IndexOutOfRangeException("Not Available!");
    }

    public static Quaterniond Inverse(Quaterniond rotation)
    {
        return new Quaterniond(-rotation.x, -rotation.y, -rotation.z, rotation.w);
    }

    public static Quaterniond Lerp(Quaterniond a, Quaterniond b, double t)
    {
        if (t > 1)
        {
            t = 1;
        }
        if (t < 0)
        {
            t = 0;
        }
        return LerpUnclamped(a, b, t);
    }

    public static Quaterniond LerpUnclamped(Quaterniond a, Quaterniond b, double t)
    {
        Quaterniond tmpQuat = new Quaterniond();
        if (Dot(a, b) < 0.0F)
        {
            tmpQuat.Set(a.x + t * (-b.x - a.x),
                        a.y + t * (-b.y - a.y),
                        a.z + t * (-b.z - a.z),
                        a.w + t * (-b.w - a.w));
        }
        else
        {
            tmpQuat.Set(a.x + t * (b.x - a.x),
                        a.y + t * (b.y - a.y),
                        a.z + t * (b.z - a.z),
                        a.w + t * (b.w - a.w));
        }
        double nor = Math.Sqrt(Dot(tmpQuat, tmpQuat));
        return new Quaterniond(tmpQuat.x / nor, tmpQuat.y / nor, tmpQuat.z / nor, tmpQuat.w / nor);
    }

    public static Quaterniond LookRotation(double3 forward)
    {
        double3 up = new double3(0, 1, 0);
        return LookRotation(forward, up);
    }

    public static Quaterniond LookRotation(double3 forward, double3 upwards)
    {
        double4x4 m = LookRotationToMatrix(forward, upwards);
        return MatrixToQuaternion(m);
    }

    public static Quaterniond RotateTowards(Quaterniond from, Quaterniond to, double maxDegreesDelta)
    {
        double num = Quaterniond.Angle(from, to);
        Quaterniond result = new Quaterniond();
        if (num == 0f)
        {
            result = to;
        }
        else
        {
            double t = Math.Min(1f, maxDegreesDelta / num);
            result = Quaterniond.SlerpUnclamped(from, to, t);
        }
        return result;
    }

    public static Quaterniond Slerp(Quaterniond a, Quaterniond b, double t)
    {
        if (t > 1)
        {
            t = 1;
        }
        if (t < 0)
        {
            t = 0;
        }
        return SlerpUnclamped(a, b, t);
    }

    public static Quaterniond SlerpUnclamped(Quaterniond q1, Quaterniond q2, double t)
    {
        double dot = Dot(q1, q2);

        Quaterniond tmpQuat = new Quaterniond();
        if (dot < 0)
        {
            dot = -dot;
            tmpQuat.Set(-q2.x, -q2.y, -q2.z, -q2.w);
        }
        else
            tmpQuat = q2;


        if (dot < 1)
        {
            double angle = Math.Acos(dot);
            double sinadiv, sinat, sinaomt;
            sinadiv = 1 / Math.Sin(angle);
            sinat = Math.Sin(angle * t);
            sinaomt = Math.Sin(angle * (1 - t));
            tmpQuat.Set((q1.x * sinaomt + tmpQuat.x * sinat) * sinadiv,
                        (q1.y * sinaomt + tmpQuat.y * sinat) * sinadiv,
                        (q1.z * sinaomt + tmpQuat.z * sinat) * sinadiv,
                        (q1.w * sinaomt + tmpQuat.w * sinat) * sinadiv);
            return tmpQuat;

        }
        else
        {
            return Lerp(q1, tmpQuat, t);
        }
    }

    public void Set(double new_x, double new_y, double new_z, double new_w)
    {
        x = new_x;
        y = new_y;
        z = new_z;
        w = new_w;
    }

    public void SetFromToRotation(double3 fromDirection, double3 toDirection)
    {
        this = FromToRotation(fromDirection, toDirection);
    }

    public void SetLookRotation(double3 view)
    {
        this = LookRotation(view);
    }

    public void SetLookRotation(double3 view, double3 up)
    {
        this = LookRotation(view, up);
    }

    public void ToAngleAxis(out double angle, out double3 axis)
    {
        angle = 2.0f * Math.Acos(w);
        if (angle == 0)
        {
            axis = new double3(1, 0, 0);
            return;
        }

        double div = 1.0f / Math.Sqrt(1 - w * w);
        axis = new double3(x * div, y * div, z * div);
        angle = angle * 180D / Math.PI;
    }

    public override string ToString()
    {
        return String.Format("({0}, {1}, {2}, {3})", x, y, z, w);
    }

    public override int GetHashCode()
    {
        return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
    }

    public override bool Equals(object other)
    {
        return this == (Quaterniond)other;
    }

    public string ToString(string format)
    {
        return String.Format("({0}, {1}, {2}, {3})", x.ToString(format), y.ToString(format), z.ToString(format), w.ToString(format));
    }

    #endregion

    #region private functions

    private double3 MatrixToEuler(double4x4 m)
    {
        double3 v = new double3();
        if (m[2][1] < 1)
        {
            if (m[2][1] > -1)
            {
                v.x = Math.Asin(-m[2][1]);
                v.y = Math.Atan2(m[2][0], m[2][2]);
                v.z = Math.Atan2(m[0][1], m[1][1]);
            }
            else
            {
                v.x = Math.PI * 0.5;
                v.y = Math.Atan2(m[1][0], m[0][0]);
                v.z = 0;
            }
        }
        else
        {
            v.x = -Math.PI * 0.5;
            v.y = Math.Atan2(-m[1][0], m[0][0]);
            v.z = 0;
        }

        for (int i = 0; i < 3; i++)
        {
            if (v[i] < 0)
            {
                v[i] += 2 * Math.PI;
            }
            else if (v[i] > 2 * Math.PI)
            {
                v[i] -= 2 * Math.PI;
            }
        }

        return v;
    }

    public static double4x4 QuaternionToMatrix(Quaterniond quat)
    {
        double x = quat.x * 2;
        double y = quat.y * 2;
        double z = quat.z * 2;
        double xx = quat.x * x;
        double yy = quat.y * y;
        double zz = quat.z * z;
        double xy = quat.x * y;
        double xz = quat.x * z;
        double yz = quat.y * z;
        double wx = quat.w * x;
        double wy = quat.w * y;
        double wz = quat.w * z;

        double4x4 m = new double4x4(
            new double4(
                1.0f - (yy + zz),
                xy + wz,
                xz - wy,
                0.0F),
            new double4(
                xy - wz,
                1.0f - (xx + zz),
                yz + wx,
                0.0F),
            new double4(
                xz + wy,
                yz - wx,
                1.0f - (xx + yy),
                0.0F),
            new double4(
                0.0F,
                0.0F,
                0.0F,
                1.0F));

        return m;
    }

    private static Quaterniond MatrixToQuaternion(double4x4 m)
    {
        Quaterniond quat = new Quaterniond();

        double fTrace = m[0][0] + m[1][1] + m[2][2];
        double root;

        if (fTrace > 0)
        {
            root = Math.Sqrt(fTrace + 1);
            quat.w = 0.5D * root;
            root = 0.5D / root;
            quat.x = (m[1][2] - m[2][1]) * root;
            quat.y = (m[2][0] - m[0][2]) * root;
            quat.z = (m[0][1] - m[1][0]) * root;
        }
        else
        {
            int[] s_iNext = new int[] { 1, 2, 0 };
            int i = 0;
            if (m[1][1] > m[0][0])
            {
                i = 1;
            }
            if (m[2][2] > m[i][i])
            {
                i = 2;
            }
            int j = s_iNext[i];
            int k = s_iNext[j];

            root = Math.Sqrt(m[i][i] - m[j][j] - m[k][k] + 1);
            if (root < 0)
            {
                throw new IndexOutOfRangeException("error!");
            }
            quat[i] = 0.5 * root;
            root = 0.5f / root;
            quat.w = (m[j][k] - m[k][j]) * root;
            quat[j] = (m[i][j] + m[j][i]) * root;
            quat[k] = (m[i][k] + m[k][i]) * root;
        }
        double nor = Math.Sqrt(Dot(quat, quat));
        quat = new Quaterniond(quat.x / nor, quat.y / nor, quat.z / nor, quat.w / nor);

        return quat;
    }

    private static double4x4 LookRotationToMatrix(double3 viewVec, double3 upVec)
    {
        double3 z = viewVec;
        double4x4 m = new double4x4();

        double mag = math.length(z);
        if (mag < 0)
        {
            m = double4x4.identity;
        }
        z /= mag;

        double3 x = math.cross(upVec, z);
        mag = math.length(x);
        if (mag < 0)
        {
            m = double4x4.identity;
        }
        x /= mag;

        double3 y = math.cross(z, x);

        m = new double4x4(
            x.x, y.x, z.x, m[3][0],
            x.y, y.y, z.y, m[3][1],
            x.z, y.z, z.z, m[3][2],
            m[0][3], m[1][3], m[2][3], m[3][3]);

        return m;
    }

    #endregion

    #region operator

    public static Quaterniond operator *(Quaterniond lhs, Quaterniond rhs)
    {
        return new Quaterniond(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                                lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
                                lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
                                lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
    }

    public static double3 operator *(Quaterniond rotation, double3 point)
    {
        double num = rotation.x * 2;
        double num2 = rotation.y * 2;
        double num3 = rotation.z * 2;
        double num4 = rotation.x * num;
        double num5 = rotation.y * num2;
        double num6 = rotation.z * num3;
        double num7 = rotation.x * num2;
        double num8 = rotation.x * num3;
        double num9 = rotation.y * num3;
        double num10 = rotation.w * num;
        double num11 = rotation.w * num2;
        double num12 = rotation.w * num3;
        double3 result;
        result.x = (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
        result.y = (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y + (num9 - num10) * point.z;
        result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (1f - (num4 + num5)) * point.z;
        return result;
    }

    public static bool operator ==(Quaterniond lhs, Quaterniond rhs)
    {
        if (lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator !=(Quaterniond lhs, Quaterniond rhs)
    {
        return !(lhs == rhs);
    }

    public static explicit operator Quaternion(Quaterniond qd) => new Quaternion((float)qd.x, (float)qd.y, (float)qd.z, (float)qd.w);
    public static implicit operator Quaterniond(Quaternion q) => new Quaterniond(q.x, q.y, q.z, q.w);
    public static explicit operator quaternion(Quaterniond qd) => new quaternion((float)qd.x, (float)qd.y, (float)qd.z, (float)qd.w);
    public static implicit operator Quaterniond(quaternion q) => new Quaterniond(q.value.x, q.value.y, q.value.z, q.value.w);

    #endregion
}
