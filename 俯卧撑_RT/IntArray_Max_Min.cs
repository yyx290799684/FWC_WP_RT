using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 玩机统计_WP_RT
{
    public class IntArray_Max_Min
    {
        /// <summary>
        /// 求int数组最大值
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int FindMax(int[] array)
        {
            int max = array[0];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > max)
                {
                    max = array[i];
                }
            }
            return max;
        }

        /// <summary>
        /// 求int数组最小值
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int FindMin(int[] array)
        {
            int min = array[0];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < min && array[i]!=0)
                {
                    min = array[i];
                }
            }
            return min;
        }
    }
}
