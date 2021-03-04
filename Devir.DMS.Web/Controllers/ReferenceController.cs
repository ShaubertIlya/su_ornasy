using System.Linq;
using System.Web.Mvc;
using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.DocumentTemplates;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.References.DynamicReferences;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Models.Reference;
using System;
using System.Collections.Generic;
using System.Web;

namespace Devir.DMS.Web.Controllers
{

    public class ReferenceController : Base.BaseController
    {
        //
        // GET: /Reference/
        #region FieldTypes
        public ActionResult FieldTypes()
        {
            return View();
        }


        public ActionResult GetDataForFieldTypes(int page, int rows, string sidx, string sord)
        {
            return new JqGridHelper<FieldType>().GetGridResult(x => !x.isDeleted, page, rows, sidx, sord);
        }

        public ActionResult GetDataForUsers()
        {
            return Json(RepositoryFactory.GetRepository<User>().List(m => m.isDeleted == false).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddFieldType()
        {
            ViewBag.isForUpdate = false;
            return View(new FieldType());
        }
        [HttpPost]
        public ActionResult AddFieldType(FieldType model)
        {
            ViewBag.isForUpdate = false;
            //if (ModelState.IsValid)
            //{
            RepositoryFactory.GetRepository<FieldType>().Insert(model);
            return JavaScript(" $('#addModal').modal('hide'); $('#list').trigger('reloadGrid');");
            //}
            //else
            //{
            //    return View(model);
            //}

        }
        public ActionResult UpdateFieldType(Guid id)
        {
            ViewBag.isForUpdate = true;
            return View("AddFieldType", RepositoryFactory.GetRepository<FieldType>().Single(m => m.Id == id));
        }
        [HttpPost]
        public ActionResult UpdateFieldType(FieldType model)
        {
            ViewBag.isForUpdate = true;
            if (ModelState.IsValid)
            {
                RepositoryFactory.GetRepository<FieldType>().update(model);
                return JavaScript(" $('#addModal').modal('hide'); $('#list').trigger('reloadGrid');");
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult DeleteFieldType(Guid id)
        {
            RepositoryFactory.GetRepository<FieldType>().Delete(id);
            return Json("ok");
        }


        public ActionResult Insert(DL.Models.References.FieldType ft)
        {
            RepositoryFactory.GetRepository<DL.Models.References.FieldType>().Insert(ft);
            return RedirectToAction("FieldTypes");
        }
        #endregion


        public ActionResult References()
        {

            var tmpDynamicReferences = RepositoryFactory.GetRepository<DynamicReference>().List(m => !m.isDeleted).ToList();
            ViewBag.DynamicReferences = tmpDynamicReferences;
            return View();
        }

        public ActionResult ModalGrid()
        {
            return View();
        }



        #region DocumentType
        public ActionResult DocumentType()
        {
            return View();
        }

        public ActionResult GetDataForDocumentType(int page, int rows, string sidx, string sord)
        {
            return new JqGridHelper<DocumentType>().GetGridResult(x => !x.isDeleted, page, rows, sidx, sord);
            //return Json(RepositoryFactory.GetRepository<DocumentType>().List(m => m.isDeleted == false).ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddDocumentType()
        {
            ViewBag.isForUpdate = false;
            return View(new DocumentType());
        }
        [HttpPost]
        public ActionResult AddDocumentType(DocumentType model)
        {
            ViewBag.isForUpdate = false;
            if (ModelState.IsValid)
            {
                RepositoryFactory.GetRepository<DocumentType>().Insert(model);
                return JavaScript("CloseCurrentModal(); $('#list').trigger('reloadGrid'); info('.top-right', 'Запись добавлена');");
            }
            else
            {
                return View(model);
            }

        }
        public ActionResult UpdateDocumentType(Guid id)
        {
            ViewBag.isForUpdate = true;
            return View("AddDocumentType", RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == id));
        }
        [HttpPost]
        public ActionResult UpdateDocumentType(DocumentType model)
        {

            ViewBag.isForUpdate = true;
            if (ModelState.IsValid)
            {
                var res = RepositoryFactory.GetRepository<DocumentType>().Single(x => x.Id == model.Id);

                model.FieldTemplates = res.FieldTemplates;
                model.RouteTemplates = res.RouteTemplates;
                model.DocumentVisualTemplate = res.DocumentVisualTemplate;

                RepositoryFactory.GetRepository<DocumentType>().update(model);
                //return JavaScript(" EditModal('Запись изменена','DocumentDetails');");
                return JavaScript("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); info('.top-right', 'Запись изменена');");
            }
            else
            {
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult DeleteDocumentType(Guid id)
        {
            RepositoryFactory.GetRepository<DocumentType>().Delete(id);
            return Json("jQuery('#list').trigger('reloadGrid'); jQuery('#details').html(''); info('.top-right', 'Запись удалена');");
        }
        public ActionResult InsertDocumentType(DocumentType ft)
        {
            RepositoryFactory.GetRepository<DocumentType>().Insert(ft);
            return RedirectToAction("DocumentType");
        }

        [HttpGet]
        public ActionResult DocumentDetails(Guid id)
        {
            var tmp = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.isDeleted == false && m.Id == id);

            tmp.RouteTemplates.ForEach(m => { m.TypeOfTheRouteId = m.TypeOfTheRoute.Id; m.DocumentFieldName = tmp.FieldTemplates.SingleOrDefault(n => n.Id == m.DocumentFieldTemplate).Header; });

            return View(tmp);
        }

        public ActionResult EditDocumentTypeRoles(Guid id)
        {
            var tmpRoles = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == id).Roles.ToList();

            var dtRvm = new DocumentTypeRolesViewModel();

            dtRvm.DocumentTypeId = id;
            dtRvm.RoleIds = tmpRoles.Select(m => m.Id).ToList();
            dtRvm.AllRoles = RepositoryFactory.GetRepository<Role>().List(m => !m.isDeleted).ToList();

            return View(dtRvm);
        }

        [HttpPost]
        public ActionResult EditDocumentTypeRoles(DocumentTypeRolesViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.SaveToDocumentType();
                return JavaScript("EditModal('Запись изменена','DocumentDetails' ,'" + model.DocumentTypeId.ToString() + "');");
            }
            else
            {
                model.AllRoles = RepositoryFactory.GetRepository<Role>().List(m => !m.isDeleted).ToList();
                return View(model);
            }
        }



        public ActionResult EditFieldsTemplate(Guid id)
        {
            var tmpFieldTemplates = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == id).FieldTemplates.ToList();
            tmpFieldTemplates.ForEach(m => m.FieldTypeId = m.FieldType.Id);
            return View(new FieldTemplateViewModel()
            {
                DocumentTypeId = id,
                FieldTemplates = tmpFieldTemplates,
                FieldTypes = RepositoryFactory.GetRepository<FieldType>().List(m => m.isDeleted == false).ToList()
            });
        }

        [HttpPost]
        public ActionResult EditFieldsTemplate(FieldTemplateViewModel data)
        {
            if (ModelState.IsValid)
            {
                FieldTemplateViewModel.SaveToDocumentType(data);
                return JavaScript("EditModal('Запись изменена','DocumentDetails' ,'" + data.DocumentTypeId.ToString() + "');");
            }
            else
            {
                data.FieldTypes = RepositoryFactory.GetRepository<FieldType>().List(m => m.isDeleted == false).ToList();
                return View(data);
            }

        }

        public ActionResult EditRouteTemplate(Guid id)
        {
            var DCtype = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == id);
            var tmpDataRT = DCtype.RouteTemplates.ToList();
            var tmpFieldTemplates = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == id).FieldTemplates.ToList();
            tmpDataRT.ForEach(m => m.TypeOfTheRouteId = m.TypeOfTheRoute.Id);



            var tmpRouteTypes = RepositoryFactory.GetRepository<RouteType>().List(m => m.isDeleted == false).ToList();
            RouteTemplateViewModel tmpViewModel = new RouteTemplateViewModel()
                                                      {
                                                          RouteTemplateId = id,
                                                          RouteTemplates = tmpDataRT,
                                                          RouteTypes = tmpRouteTypes,
                                                          FieldTemplates = tmpFieldTemplates

                                                      };

            return View(tmpViewModel);
        }
        [HttpPost]
        public ActionResult EditRouteTemplate(RouteTemplateViewModel data)
        {
            if (ModelState.IsValid)
            {

                data.RouteTemplates.ForEach(m =>
                {
                    m.FieldOrder = data.RouteTemplates.IndexOf(m);
                });



                RouteTemplateViewModel.SaveToDocumentType(data);
                return JavaScript("EditModal('Запись изменена', 'DocumentDetails','" + data.RouteTemplateId.ToString() + "');");
            }
            else
            {
                //data.RouteTemplates = RepositoryFactory.GetRepository<RouteTemplate>().List(m => m.isDeleted == false).ToList();
                return View(data);
            }
        }

        [HttpGet]
        public ActionResult AddFieldTemplate()
        {
            return View();
        }


        #endregion

        public ActionResult DocumentVisualConstructor(Guid DocumentTypeId)
        {
            var model = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == DocumentTypeId);
            return View(model);
        }

        public ActionResult SaveDocumentVisualStructure(DocumentVisualTemplate model)
        {
            var docType = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == model.DocumentTypeId);
            docType.DocumentVisualTemplate = model.Blocks;
            RepositoryFactory.GetRepository<DocumentType>().update(docType);
            return JavaScript("ok");
        }

        #region RouteType
        public ActionResult RouteType()
        {
            return View();
        }
        public ActionResult GetDataForRouteType()
        {
            return Json(RepositoryFactory.GetRepository<RouteType>().List(m => m.isDeleted == false).ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddRouteType()
        {
            ViewBag.isForUpdate = false;
            return View(new RouteType());
        }
        [HttpPost]
        public ActionResult AddRouteType(RouteType model)
        {
            ViewBag.isForUpdate = false;
            if (ModelState.IsValid)
            {
                RepositoryFactory.GetRepository<RouteType>().Insert(model);
                return JavaScript("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); EditModal('Запись добавлена','DocumentDetails');");
            }
            else
            {
                return View(model);
            }

        }
        public ActionResult UpdateRouteType(Guid id)
        {
            ViewBag.isForUpdate = true;
            return View("AddRouteType", RepositoryFactory.GetRepository<RouteType>().Single(m => m.Id == id));
        }
        [HttpPost]
        public ActionResult UpdateRouteType(RouteType model)
        {
            ViewBag.isForUpdate = true;
            if (ModelState.IsValid)
            {
                model.Actions = RepositoryFactory.GetRepository<RouteType>().Single(x => x.Id == model.Id).Actions;

                RepositoryFactory.GetRepository<RouteType>().update(model);
                return JavaScript("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); info('.top-right', 'Запись изменена');");
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult DeleteRouteType(Guid id)
        {
            RepositoryFactory.GetRepository<RouteType>().Delete(id);
            //return Json("Запись удалена");
            return Json("jQuery('#list').trigger('reloadGrid'); jQuery('#details').html(''); info('.top-right', 'Запись удалена');");
        }
        public ActionResult InsertRouteType(RouteType ft)
        {
            RepositoryFactory.GetRepository<RouteType>().Insert(ft);
            return RedirectToAction("RouteType");
        }

        public ActionResult RouteTypeActions(Guid id)
        {
            return View(RepositoryFactory.GetRepository<RouteType>().Single(m => m.isDeleted == false && m.Id == id));
        }

        public ActionResult EditeRouteAction(Guid id)
        {
            var tmpData =
                RepositoryFactory.GetRepository<RouteType>().Single(m => m.Id == id && m.isDeleted == false).Actions;

            RouteTypeActionsViewModel tmpViewModel = new RouteTypeActionsViewModel()
            {
                RouteActions = tmpData != null ? tmpData.Select(m => new RouteActionViewModel() { RouteActionId = m.Id }).ToList() : null,
                RouteActionsList =
                    RepositoryFactory.GetRepository<RouteAction>().List(
                        m => m.isDeleted == false).ToList(),
                RouteTypeId = id
            };


            return View(tmpViewModel);
        }

        [HttpPost]
        public ActionResult EditeRouteAction(RouteTypeActionsViewModel data)
        {
            if (ModelState.IsValid)
            {
                var tmpRouteType = RepositoryFactory.GetRepository<RouteType>().Single(m => m.Id == data.RouteTypeId);

                tmpRouteType.Actions = RepositoryFactory.GetRepository<RouteAction>().List(m => m.isDeleted == false).ToList().Where(m => data.RouteActions.Where(n => n.RouteActionId == m.Id).Count() > 0).ToList();

                RepositoryFactory.GetRepository<RouteType>().update(tmpRouteType);

                return JavaScript("EditModal('Запись изменена','DocumentDetails');");
            }
            else
            {
                // data.RouteActions = RepositoryFactory.GetRepository<RouteAction>().List(m => m.isDeleted == false).ToList();
                return View(data);
            }

        }


        #endregion

        #region RouteActions
        public ActionResult RouteActions()
        {
            return View();
        }
        public ActionResult GetDataForRouteAction(int page, int rows, string sidx, string sord)
        {
            return new JqGridHelper<RouteAction>().GetGridResult(x => !x.isDeleted, page, rows, sidx, sord);

            //return Json(RepositoryFactory.GetRepository<RouteAction>().List(m => m.isDeleted == false).ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddRouteAction()
        {
            ViewBag.isForUpdate = false;
            return View(new RouteAction());
        }
        [HttpPost]
        public ActionResult AddRouteAction(RouteAction model)
        {
            ViewBag.isForUpdate = false;
            if (ModelState.IsValid)
            {
                RepositoryFactory.GetRepository<RouteAction>().Insert(model);
                return JavaScript("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); info('.top-right', 'Запись добавлена');");
            }
            else
            {
                return View(model);
            }

        }
        public ActionResult UpdateRouteAction(Guid id)
        {
            ViewBag.isForUpdate = true;
            return View("AddRouteAction", RepositoryFactory.GetRepository<RouteAction>().Single(m => m.Id == id));
        }
        [HttpPost]
        public ActionResult UpdateRouteAction(RouteAction model)
        {
            ViewBag.isForUpdate = true;
            if (ModelState.IsValid)
            {
                RepositoryFactory.GetRepository<RouteAction>().update(model);
                return JavaScript("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); info('.top-right', 'Запись изменена');");
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult DeleteRouteAction(Guid id)
        {
            RepositoryFactory.GetRepository<RouteAction>().Delete(id);
            return Json("jQuery('#list').trigger('reloadGrid'); jQuery('#details').html(''); info('.top-right', 'Запись удалена');");
            //return Json("Запись удалена");
        }
        public ActionResult InsertRouteAction(RouteAction ft)
        {
            RepositoryFactory.GetRepository<RouteAction>().Insert(ft);
            return RedirectToAction("RouteActions");
        }
        #endregion

        #region DynamicReference
        public ActionResult DynamicReference()
        {
            return View();
        }

        public ActionResult GetDataForDynamicReference()
        {
            //Проверка
            return Json(RepositoryFactory.GetRepository<DynamicReference>().List(m => m.isDeleted == false).ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddDynamicReference()
        {
            ViewBag.isForUpdate = false;
            return View(new DynamicReference());
        }

        [HttpPost]
        public ActionResult AddDynamicReference(DynamicReference model)
        {
            ViewBag.isForUpdate = false;
            if (ModelState.IsValid)
            {
                RepositoryFactory.GetRepository<DynamicReference>().Insert(model);
                RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
                {
                    Name = String.Format("Справочник: {0}", model.Name),
                    DynamicReferenceId = model.Id,
                    SystemName = String.Format("References.{0}", model.Name),
                    Comment = String.Format("Создано автоматически на основании динамического справочника {0}", model.Name)
                });
                return JavaScript("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); RefreshLeftMenu(); info('.top-right', 'Запись добавлена');");
                //return JavaScript(" EditModal('Запись добавлена');");
            }
            else
            {
                return View(model);
            }

        }
        public ActionResult UpdateDynamicReference(Guid id)
        {
            ViewBag.isForUpdate = true;
            return View("AddDynamicReference", RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == id));
        }
        [HttpPost]
        public ActionResult UpdateDynamicReference(DynamicReference model)
        {
            ViewBag.isForUpdate = true;
            if (ModelState.IsValid)
            {
                model.FieldTemplates =
                    RepositoryFactory.GetRepository<DynamicReference>().Single(x => x.Id == model.Id).FieldTemplates;

                RepositoryFactory.GetRepository<DynamicReference>().update(model);
                return JavaScript("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); info('.top-right', 'Запись изменена');");
                //return JavaScript(" EditModal('Запись изменена','DynamicReferenceDetails');");
            }
            else
            {
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult DeleteDynamicReference(Guid id)
        {
            RepositoryFactory.GetRepository<DynamicReference>().Delete(id);
            return Json("jQuery('#list').trigger('reloadGrid'); jQuery('#details').html(''); RefreshLeftMenu(); info('.top-right', 'Запись удалена');");
            //return Json("Запись удалена");
        }

        [HttpGet]
        public ActionResult DynamicReferenceDetails(Guid id)
        {
            var tmp = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.isDeleted == false && m.Id == id);

            return View(tmp);
        }

        public ActionResult EditDynamicFieldsTemplate(Guid id)
        {
            var tmpFieldTemplates = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == id).FieldTemplates.ToList();
            tmpFieldTemplates.ForEach(m => m.TypeOfTheFieldId = m.TypeOfTheField.Id);
            return View(new DynamicFieldsTemplateViewModel()
            {
                DynamicReferenceId = id,
                FieldTemplates = tmpFieldTemplates,
                FieldTypes = RepositoryFactory.GetRepository<FieldType>().List(m => m.isDeleted == false && m.Id != new Guid("b33f3a3c-e9dd-4fb3-9b0a-00a79870954a")).ToList()
                //DocumentTypeId = id,
                //FieldTemplates = tmpFieldTemplates,
                //FieldTypes = RepositoryFactory.GetRepository<DynamicReference>().List(m => m.isDeleted == false).ToList()
            });
        }

        [HttpPost]
        public ActionResult EditDynamicFieldsTemplate(DynamicFieldsTemplateViewModel data)
        {
            if (ModelState.IsValid)
            {
                DynamicFieldsTemplateViewModel.SaveToDynamicReference(data);
                return JavaScript("EditModal('Запись изменена','DynamicReferenceDetails','" + data.DynamicReferenceId.ToString() + "');");
            }
            else
            {
                data.FieldTypes = RepositoryFactory.GetRepository<FieldType>().List(m => m.isDeleted == false).ToList();
                return View(data);
            }

        }

        public ActionResult ShowDynamicReference(Guid id, string dynRefname = "", bool isDialog = false, string addModal = "")
        {
            var tmpDynamicReference = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == id);

            var ft = tmpDynamicReference.FieldTemplates.SingleOrDefault(m => m.isDisplay);
            if (ft != null)
            {
                var tmpDisplayFieldName = ft.Header;
                ViewBag.displayFieldName = tmpDisplayFieldName;
            }

            ViewBag.isDialog = isDialog;
            ViewBag.addModal = addModal;
            ViewBag.dynRefname = dynRefname;
            return View(tmpDynamicReference);
        }

        public ActionResult GetDataForDynamicRecord(Guid Id, int page, int rows, string sidx, string sord)
        {
            //return new JqGridHelper<DynamicRecord>().GetGridResultForDynamicRef(m => !m.isDeleted && m.DynamicReferenceId == Id, page, rows, sidx, sord);

            var tmpList = RepositoryFactory.GetRepository<DynamicRecord>().List(m => !m.isDeleted && m.DynamicReferenceId == Id).
                GroupBy(m => m.RecordId).
                Select(m => new { Id = m.Key, value = m.ToList() });

            JsonResult j = new JsonResult();
            j.Data = tmpList; //new JqGridHelper().GetGridResult(tmpList, page, rows); // Json(tmpList, JsonRequestBehavior.AllowGet);
            return j;
        }

        public ActionResult AddDynamicRecord(Guid DynamicReferenceId)
        {
            ViewBag.isForUpdate = false;

            var tmpDynamicTemplate = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == DynamicReferenceId);

            DynamicRecordViewModel drVm = new DynamicRecordViewModel();

            drVm.ReferenceId = tmpDynamicTemplate.Id;
            drVm.Fields = tmpDynamicTemplate.FieldTemplates.Select(m => new DynamicRecordFieldViewModel()
            {
                DynamicFieldTemplateId = m.Id,
                Header = m.Header,
                isRequired = m.isRequired,
                TypeOfTheFieldId = m.TypeOfTheField.Id,
                Value = m.TypeOfTheField.Id == new Guid("2490becb-3476-43ab-8717-0f0b138a6ab2") ? "False" : "",
                DynamicReferenceId = m.TypeOfTheField.DynamicReferenceId,

            }).ToList();

            return View(drVm);
        }

        [HttpPost]
        public ActionResult AddDynamicRecord(DynamicRecordViewModel model)
        {
            ViewBag.isForUpdate = false;
            var s = ModelState;

            for (int i = 0; i < model.Fields.Count(); i++)
            {
                //Проверка на Required


                if (String.IsNullOrEmpty(model.Fields[i].Value) && model.Fields[i].isRequired)
                {
                    ModelState.AddModelError(String.Format("Fields[{0}].Value", i), "Необходимо заполнить поле");
                }
                else
                {

                    //Проверка на тип данных
                    if (!BL.DynamicRecords.DataTypeHelper.CheckFieldForDataType(model.ReferenceId, model.Fields[i].DynamicFieldTemplateId, model.Fields[i].Value))
                    {
                        ModelState.AddModelError(String.Format("Fields[{0}].Value", i), "Значение поля не соответсвует формату");
                    }
                }


            }

            if (ModelState.IsValid)
            {
                model.InsertToDynamicReference();
                return JavaScript("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); jQuery('#listModal').trigger('reloadGrid'); info('.top-right', 'Запись добавлена');");
            }
            else
                return View(model);
        }

        public ActionResult UpdateDynamicRecord(Guid DynamicReferenceId, Guid Id)
        {

            return null;
        }


        public ActionResult EditDynamicRecord(Guid DynamicReferenceId, Guid Id)
        {
            ViewBag.isForUpdate = true;

            var tmpDynamicTemplate = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == DynamicReferenceId);

            DynamicRecordViewModel drVm = new DynamicRecordViewModel();

            drVm.RecordId = Id;
            drVm.ReferenceId = tmpDynamicTemplate.Id;
            drVm.Fields = tmpDynamicTemplate.FieldTemplates.Select(m => new DynamicRecordFieldViewModel()
            {
                Id = RepositoryFactory.GetRepository<DynamicRecord>().Single(n => n.RecordId == Id && n.DynamicReferenceFieldTemplateId == m.Id && n.Value.Value.FieldTypeId == m.TypeOfTheField.Id && !n.isDeleted).Id,
                DynamicFieldTemplateId = m.Id,
                Header = m.Header,
                isRequired = m.isRequired,
                TypeOfTheFieldId = m.TypeOfTheField.Id,
                Value = RepositoryFactory.GetRepository<DynamicRecord>().Single(n => n.RecordId == Id && n.DynamicReferenceFieldTemplateId == m.Id && n.Value.Value.FieldTypeId == m.TypeOfTheField.Id && !n.isDeleted).Value.Value.ValueToDisplay,
                ValueId = RepositoryFactory.GetRepository<DynamicRecord>().Single(n => n.RecordId == Id && n.DynamicReferenceFieldTemplateId == m.Id && n.Value.Value.FieldTypeId == m.TypeOfTheField.Id && !n.isDeleted).Value.Value.DynamicRecordId,
                DynamicReferenceId = m.TypeOfTheField.DynamicReferenceId,
            }).ToList();

            return View("AddDynamicRecord", drVm);
        }


        [HttpPost]
        public ActionResult UpdateDynamicRecord(DynamicRecordViewModel model)
        {
            ViewBag.isForUpdate = false;
            var s = ModelState;

            for (int i = 0; i < model.Fields.Count(); i++)
            {
                //Проверка на Required


                if (String.IsNullOrEmpty(model.Fields[i].Value) && model.Fields[i].isRequired)
                {
                    ModelState.AddModelError(String.Format("Fields[{0}].Value", i), "Необходимо заполнить поле");
                }
                else
                {

                    //Проверка на тип данных
                    if (!BL.DynamicRecords.DataTypeHelper.CheckFieldForDataType(model.ReferenceId, model.Fields[i].DynamicFieldTemplateId, model.Fields[i].Value))
                    {
                        ModelState.AddModelError(String.Format("Fields[{0}].Value", i), "Значение поля не соответсвует формату");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                model.UpdateInDynamicReference();
                return JavaScript("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); info('.top-right', 'Запись изменена');");
            }
            else
                return View(model);
        }

        [HttpPost]
        public ActionResult DeleteDynamicRecord(Guid Id)
        {
            RepositoryFactory.GetRepository<DynamicRecord>().List(m => m.RecordId == Id).ToList().ForEach(m =>
            {
                RepositoryFactory.GetRepository<DynamicRecord>().Delete(m.Id);
            });

            return Json("CloseCurrentModal(); jQuery('#list').trigger('reloadGrid'); info('.top-right', 'Запись удалена');");
        }


        public ActionResult GetDynamicReferenceName()
        {
            return Json(RepositoryFactory.GetRepository<DynamicReference>().List(m => m.isDeleted == false)
                .Select(x => new { name = x.Name, id = x.Id }).ToList(), JsonRequestBehavior.AllowGet);
        }


        //public ActionResult Test(int rows = 10, int page = 1, string sort = "Id", string order = "asc")
        //{




        //    //var users = RepositoryFactory.GetRepository<User>().List(u => !u.isDeleted).Select(u2 => new
        //    //    {
        //    //        itemid = u2.Id,
        //    //            productid = u2.UserId,
        //    //            listprice = u2.FirstName,
        //    //            unitcost = u2.LastName,
        //    //            attr = u2.FatherName,
        //    //            status = u2.Email
        //    //    });


        //    return new JqGridHelper<FieldType>().GetDataForDataGrid(u2 => !u2.isDeleted, page, rows, sort, order);
        //}

        #endregion



        
    }
}

