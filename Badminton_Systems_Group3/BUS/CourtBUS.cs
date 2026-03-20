using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Badminton_Systems_Group3.BUS
{
    internal class CourtBUS
    {
        private CourtDAL dal = new CourtDAL();

        public List<CourtDTO> GetAll()
        {
            return dal.GetAll();
        }

        public bool Insert(CourtDTO sb)
        {
            return dal.Insert(sb);
        }

        public bool Update(CourtDTO sb)
        {
            return dal.Update(sb);
        }

        public bool Delete(string maSan)
        {
            return dal.Delete(maSan);
        }
    }
}
