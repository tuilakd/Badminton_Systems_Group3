using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Badminton_Systems_Group3.BUS
{
    internal class SanBaiBUS
    {
        private SanBaiDAL dal = new SanBaiDAL();

        public List<SanBaiDTO> GetAll()
        {
            return dal.GetAll();
        }

        public bool Insert(SanBaiDTO sb)
        {
            return dal.Insert(sb);
        }

        public bool Update(SanBaiDTO sb)
        {
            return dal.Update(sb);
        }

        public bool Delete(string maSan)
        {
            return dal.Delete(maSan);
        }
    }
}
