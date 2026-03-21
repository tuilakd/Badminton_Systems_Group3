using System.Collections.Generic;
using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;

namespace Badminton_Systems_Group3.BUS
{
    public class ClientBUS
    {
        ClientDAL dal = new ClientDAL();

        public List<ClientDTO> GetAll()
        {
            return dal.GetAll();
        }

        public void Add(ClientDTO kh)
        {
            dal.Insert(kh);
        }

        public void Delete(string maKH)
        {
            dal.Delete(maKH);
        }

        public void Update(ClientDTO kh)
        {
            dal.Update(kh);
        }
        public List<ClientDTO> Search(string keyword)
        {
            return dal.Search(keyword);
        }
        public bool Exists(string maKH)
        {
            return dal.CheckExist(maKH);
        }
        
        
    }
}