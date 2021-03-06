
/**
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：后台管理员                                                    
*│　作    者：yilezhu                                            
*│　版    本：1.0    模板代码自动生成                                                
*│　创建时间：2018-12-31 16:43:28                             
*└──────────────────────────────────────────────────────────────┘
*┌──────────────────────────────────────────────────────────────┐
*│　命名空间： Czar.Cms.Services                                  
*│　类    名： ManagerService                                    
*└──────────────────────────────────────────────────────────────┘
*/
using AutoMapper;
using Czar.Cms.Core.Extensions;
using Czar.Cms.Core.Helper;
using Czar.Cms.IRepository;
using Czar.Cms.IServices;
using Czar.Cms.Models;
using Czar.Cms.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Czar.Cms.Services
{
    public class ManagerService: IManagerService
    {
        private readonly IManagerRepository _repository;
        private readonly IManagerRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public ManagerService(IManagerRepository repository, IManagerRoleRepository roleRepository, IMapper mapper)
        {
            _repository = repository;
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public BaseResult AddOrModify(ManagerAddOrModifyModel item)
        {
            var result = new BaseResult();
            Manager manager;
            if (item.Id == 0)
            {
                //TODO ADD
                manager = _mapper.Map<Manager>(item);
                manager.Password = AESEncryptHelper.Encode(CzarCmsKeys.DefaultPassword, CzarCmsKeys.AesEncryptKeys);
                manager.LoginCount = 0;
                manager.AddManagerId = 1;
                manager.IsDelete = false;
                manager.AddTime = DateTime.Now;
                if (_repository.Insert(manager) > 0)
                {
                    result.ResultCode = ResultCodeAddMsgKeys.CommonObjectSuccessCode;
                    result.ResultMsg = ResultCodeAddMsgKeys.CommonObjectSuccessMsg;
                }
                else
                {
                    result.ResultCode = ResultCodeAddMsgKeys.CommonExceptionCode;
                    result.ResultMsg = ResultCodeAddMsgKeys.CommonExceptionMsg;
                }
            }
            else
            {
                //TODO Modify
                manager = _repository.Get(item.Id);
                if (manager != null)
                {
                    _mapper.Map(item, manager);
                    manager.ModifyManagerId = 1;
                    manager.ModifyTime = DateTime.Now;
                    if (_repository.Update(manager) > 0)
                    {
                        result.ResultCode = ResultCodeAddMsgKeys.CommonObjectSuccessCode;
                        result.ResultMsg = ResultCodeAddMsgKeys.CommonObjectSuccessMsg;
                    }
                    else
                    {
                        result.ResultCode = ResultCodeAddMsgKeys.CommonExceptionCode;
                        result.ResultMsg = ResultCodeAddMsgKeys.CommonExceptionMsg;
                    }
                }
                else
                {
                    result.ResultCode = ResultCodeAddMsgKeys.CommonFailNoDataCode;
                    result.ResultMsg = ResultCodeAddMsgKeys.CommonFailNoDataMsg;
                }
            }
            return result;
        }

        public BaseResult DeleteIds(int[] Ids)
        {
            var result = new BaseResult();
            if (Ids.Count() == 0)
            {
                result.ResultCode = ResultCodeAddMsgKeys.CommonModelStateInvalidCode;
                result.ResultMsg = ResultCodeAddMsgKeys.CommonModelStateInvalidMsg;

            }
            else
            {
                var count = _repository.DeleteLogical(Ids);
                if (count > 0)
                {
                    //成功
                    result.ResultCode = ResultCodeAddMsgKeys.CommonObjectSuccessCode;
                    result.ResultMsg = ResultCodeAddMsgKeys.CommonObjectSuccessMsg;
                }
                else
                {
                    //失败
                    result.ResultCode = ResultCodeAddMsgKeys.CommonExceptionCode;
                    result.ResultMsg = ResultCodeAddMsgKeys.CommonExceptionMsg;
                }


            }
            return result;
        }

        public TableDataModel LoadData(ManagerRequestModel model)
        {
            string conditions = "where IsDelete=0 ";//未删除的
            if (!model.Key.IsNullOrWhiteSpace())
            {
                conditions += $"and (UserName like '%{model.Key}%' or NickName like '%{model.Key}%' or Remark like '%{model.Key}%' or Mobile like '%{model.Key}%' or Email like '%{model.Key}%')";
            }
            var list =_repository.GetListPaged(model.Page, model.Limit, conditions, "Id desc").ToList();
            var viewList = new List<ManagerListModel>();
            list.ForEach(x=> {
                var item = _mapper.Map<ManagerListModel>(x);
                item.RoleName = _roleRepository.GetNameById(x.RoleId);
                viewList.Add(item);
            });
            return new TableDataModel
            {
                count = _repository.RecordCount(conditions),
                data = viewList,
            };
        }

        public BaseResult ChangeLockStatus(ManagerChangeLockStatusModel model)
        {
            var result = new BaseResult();
            //判断状态是否发生变化，没有则修改，有则返回状态已变化无法更改状态的提示
            var isLock = _repository.GetLockStatusById(model.Id);
            if (isLock == !model.IsLock)
            {
                var count = _repository.ChangeLockStatusById(model.Id,model.IsLock);
                if (count > 0)
                {
                    result.ResultCode = ResultCodeAddMsgKeys.CommonObjectSuccessCode;
                    result.ResultMsg = ResultCodeAddMsgKeys.CommonObjectSuccessMsg;
                }
                else
                {
                    result.ResultCode = ResultCodeAddMsgKeys.CommonExceptionCode;
                    result.ResultMsg = ResultCodeAddMsgKeys.CommonExceptionMsg;
                }
            }
            else
            {
                result.ResultCode = ResultCodeAddMsgKeys.CommonDataStatusChangeCode;
                result.ResultMsg = ResultCodeAddMsgKeys.CommonDataStatusChangeMsg;
            }
            return result;
        }
    }
}