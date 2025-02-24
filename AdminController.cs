using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Models;
using System.Dynamic;
using System.Data.Entity;
using System.Collections.Generic;           
using Org.BouncyCastle.Crypto.Generators;
using System.Drawing;





namespace WebApplication4.Controllers
{

    public class AdminController : Controller
    {
        private readonly dummyclubsEntities _db = new dummyclubsEntities();
        private readonly EmailService _emailService = new EmailService();  // Injecting EmailService


        // GET: Login Page
        public ActionResult Login()
        {
            return View();
        }

        // POST: Handle Login
        [HttpPost]

        // POST: Handle Login

    
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check credentials in Logins table
                var user = _db.Logins.FirstOrDefault(u => u.Email == model.Username && u.PasswordHash == model.Password);

                if (user != null)
                {
                    // Store user session data
                    Session["UserID"] = user.LoginID;
                    Session["UserRole"] = user.Role;
                    Session["UserEmail"] = user.Email;

                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Adminmethod", "AdminDashboard");
                    }
                    else if (user.Role == "UniversityAdministrator")
                    {
                        // Fetch assigned university
                        var university = _db.UNIVERSITies.FirstOrDefault(u => u.Email == user.Email);

                        if (university != null)
                        {
                            // Store university details in session
                            Session["UniversityID"] = university.UniversityID;
                            Session["UniversityName"] = university.UniversityNAME;
                            Session["UniversityLocation"] = university.Location;

                            // Redirect to University Admin Dashboard
                            return RedirectToAction("Index", "UniversityAdmin");
                        }
                        else
                        {
                            ViewBag.Message = "No university assigned to this administrator.";
                        }
                    }
                    else if (user.Role == "Mentor")
                    {
                        // Store the mentor's university ID in the session
                        Session["UniversityID"] = user.UniversityID;

                        // Optionally, you can store other details like the mentor's ID or role
                        Session["UserID"] = user.LoginID;
                        Session["UserRole"] = user.Role;


                        return RedirectToAction("Index", "Mentor");

                    }
                    else
                    {
                        ViewBag.Message = "Access Denied! Invalid Role.";
                    }
                }
                else
                {
                    ViewBag.Message = "Invalid email or password.";
                }
            }
            return View(model);
        }


        // Logout Action
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // Add Mentor Action
        //public ActionResult AddMentor()
        //{
        //    return View();
        //}


        //[HttpPost]
        /*public ActionResult AddMentor(USER model, HttpPostedFileBase Photo)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ViewBag.Errors = errors;
                return View(model);
            }

            try
            {
                if (Photo != null && Photo.ContentLength > 0)
                {
                    string uploadDir = Server.MapPath("~/Uploads/");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string fileName = Path.GetFileName(Photo.FileName);
                    string path = Path.Combine(uploadDir, fileName);
                    Photo.SaveAs(path);
                    model.PhotoPath = "~/Uploads/" + fileName;
                }

                // Save model to the database
                _db.USERs.Add(model);
                _db.SaveChanges();

                

                // Store success message in TempData
               // TempData["SuccessMessage"] = "Mentor added successfully!";

                // Clear the model (this is optional, but it clears the form)
                model = new USER();  // Resets the model to clear form data

                // Optionally clear any session or other temporary data
                Session.Clear(); // Clears the session data (if needed)

                // Redirect to the same view (this will reload the page with the success message)
                return RedirectToAction("successmentor");
            }
            catch (Exception ex)
            {
                // Log exception for debugging
                ViewBag.ErrorMessage = "Error: " + ex.Message;
                return View(model);
            }
        }*/


        //public async Task<ActionResult> AddMentor(USER model, HttpPostedFileBase Photo)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //        ViewBag.Errors = errors;
        //        return View(model);
        //    }

        //    try
        //    {
        //        // Upload photo if provided
        //        if (Photo != null && Photo.ContentLength > 0)
        //        {
        //            string uploadDir = Server.MapPath("~/Uploads/");
        //            if (!Directory.Exists(uploadDir))
        //            {
        //                Directory.CreateDirectory(uploadDir);
        //            }

        //            string fileName = Path.GetFileName(Photo.FileName);
        //            string path = Path.Combine(uploadDir, fileName);
        //            Photo.SaveAs(path);
        //            model.PhotoPath = "~/Uploads/" + fileName;
        //        }

        //        // Save model to the database
        //        _db.USERs.Add(model);
        //        await _db.SaveChangesAsync();  // Use async for better performance

        //        // Send the email to the mentor with their credentials
        //        string subject = "Welcome to Our Platform!";
        //        string body = $"Hello {model.FirstName},<br/><br/>" +
        //                      $"You have been successfully added as a mentor. Here are your login details:<br/>" +
        //                      $"Username: {model.Email}<br/>" +
        //                      $"Password: {model.Password}<br/><br/>" +
        //                      "Please login and complete your profile.";

        //        // Send email asynchronously
        //        await _emailService.SendEmailAsync(model.Email, subject, body);

        //        // Clear the model (optional, clears the form data)
        //        model = new USER();

        //        // Optionally clear any session or other temporary data
        //        Session.Clear();

        //        // Redirect to success page
        //        return RedirectToAction("successmentor");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error and display error message
        //        ViewBag.ErrorMessage = "Error: " + ex.Message;
        //        return View(model);
        //    }
        //}

        public ActionResult successmentor()
        {
            return View();
        }

        // ✅ Show Add University Form
        public ActionResult AddUniversity()
        {
            return View();
        }

        // ✅ Handle University Submission
        [HttpPost]
        // POST: Handle Add University Submission

        
        public async Task<ActionResult> AddUniversity(UNIVERSITY model)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                        }
                    }
                }
                return View(model);
            }

            try
            {
                // Set default values
                model.CreatedDate = model.CreatedDate ?? DateTime.Now;
                model.IsActive = model.IsActive ?? true;
                model.IsActiveDate = model.IsActiveDate ?? DateTime.Now;

                // Save University
                _db.UNIVERSITies.Add(model);
                await _db.SaveChangesAsync();

                // Add the administrator in the Logins table (Storing Plain-Text Password)
                var login = new Login
                {
                    Email = model.Email,
                    PasswordHash = "Administrator@123", // ⚠️ Storing password as plain text (Not Recommended)
                    Role = "UniversityAdministrator",
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    UniversityID = model.UniversityID // Assign the foreign key
                };

                _db.Logins.Add(login);
                await _db.SaveChangesAsync();

                // Send email to the administrator with login credentials
                string subject = "Welcome to Our Platform!";
                string body = $"Hello {model.AdministratorName},<br/><br/>" +
                              $"You have been successfully added as the administrator for {model.UniversityNAME}.<br/>" +
                              $"Your login credentials are:<br/>" +
                              $"Username: {model.Email}<br/>" +
                              $"Password: Administrator@123<br/><br/>" +
                              "Please login and manage your university.";

                // Send email asynchronously
                await _emailService.SendEmailAsync(model.Email, subject, body);

                // Success message
                TempData["SuccessMessage"] = "University and administrator added successfully!";
                return RedirectToAction("ManageUniversities");
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ViewBag.ErrorMessage = "Error: " + ex.Message;
                return View(model);
            }
        }


        // ✅ Success Page
        public ActionResult SuccessUniversity()
        {
            return View();
        }

        public ActionResult AddDepartment()
        {
            // Fetch only active universities
            ViewBag.Universities = _db.UNIVERSITies.Where(u => u.IsActive == true).ToList();
            return View(new List<DEPARTMENT>());
        }


        [HttpPost]
       
        public async Task<ActionResult> AddDepartment(List<DEPARTMENT> Departments, int Universityid)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(Departments);
            }

            try
            {
                foreach (var department in Departments)
                {
                    department.Universityid = Universityid;
                    department.createdDate = DateTime.Now;
                    department.IsActive = true;
                    department.IsActiveDate = DateTime.Now;

                    _db.DEPARTMENTs.Add(department);
                }

                await _db.SaveChangesAsync();

                // ✅ Store success message in TempData
                TempData["SuccessMessage"] = "Departments added successfully!";

                // ✅ Clear Model Data
                ModelState.Clear();

                return RedirectToAction("AddDepartment"); // Reloads the same page to show success message
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
                return View(Departments);
            }
        }


        // ✅ Show Manage Universities Page
        public ActionResult ManageUniversities()
        {
            var universities = _db.UNIVERSITies.OrderByDescending(u => u.CreatedDate).ToList();
            return View(universities);
        }

        // ✅ Handle Deactivating a University
        [HttpPost]
        public async Task<ActionResult> DeactivateUniversity(int universityId)
        {
            var university = await _db.UNIVERSITies.FindAsync(universityId);
            if (university == null)
            {
                TempData["ErrorMessage"] = "University not found!";
                return RedirectToAction("ManageUniversities");
            }

            university.IsActive = false;
            university.IsActiveDate = DateTime.Now;

            var departments = _db.DEPARTMENTs.Where(d => d.Universityid == universityId).ToList();
            foreach (var dept in departments)
            {
                dept.IsActive = false;
                dept.IsActiveDate = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "University and its departments have been deactivated!";
            return RedirectToAction("ManageUniversities");
        }

        // ✅ Handle Activating a University
        [HttpPost]
        public async Task<ActionResult> ActivateUniversity(int universityId)
        {
            var university = await _db.UNIVERSITies.FindAsync(universityId);
            if (university == null)
            {
                TempData["ErrorMessage"] = "University not found!";
                return RedirectToAction("ManageUniversities");
            }

            university.IsActive = true;
            university.IsActiveDate = DateTime.Now;

            var departments = _db.DEPARTMENTs.Where(d => d.Universityid == universityId).ToList();
            foreach (var dept in departments)
            {
                dept.IsActive = true;
                dept.IsActiveDate = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "University and its departments have been activated!";
            return RedirectToAction("ManageUniversities");
        }

        // ✅ View All Universities (Active & Deactivated)
        public ActionResult ViewUniversities()
        {
            var universities = _db.UNIVERSITies.ToList();
            return View(universities);
        }

        // ✅ View Only Active Universities
        public ActionResult ViewActiveUniversities()
        {
            var universities = _db.UNIVERSITies.Where(u => u.IsActive == true).ToList();
            return View("ViewUniversities", universities);
        }

        // ✅ View Only Deactivated Universities
        public ActionResult ViewDeactivatedUniversities()
        {
            var universities = _db.UNIVERSITies.Where(u => u.IsActive == false).ToList();
            return View("ViewUniversities", universities);
        }


        // Manage Schools (Departments)
        public ActionResult ManageSchools()
        {
            var departments = _db.DEPARTMENTs.Include(d => d.UNIVERSITY).ToList(); // Make sure to include University data
            return View(departments);
        }

        // Deactivate Department
        [HttpPost]
        public async Task<ActionResult> DeactivateDepartment(int departmentId)
        {
            try
            {
                var department = await _db.DEPARTMENTs.FindAsync(departmentId);

                if (department == null)
                {
                    TempData["ErrorMessage"] = "Department not found!";
                    return RedirectToAction("ManageSchools");
                }

                department.IsActive = false;
                department.IsActiveDate = DateTime.Now;

                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Department deactivated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction("ManageSchools");
        }

        // Activate Department
        [HttpPost]
        public async Task<ActionResult> ActivateDepartment(int departmentId)
        {
            try
            {
                var department = await _db.DEPARTMENTs.FindAsync(departmentId);

                if (department == null)
                {
                    TempData["ErrorMessage"] = "Department not found!";
                    return RedirectToAction("ManageSchools");
                }

                department.IsActive = true;
                department.IsActiveDate = DateTime.Now;

                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Department activated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction("ManageSchools");
        }


        // Show All Schools (Departments)
        public ActionResult ViewSchools()
        {
            var departments = _db.DEPARTMENTs.Include(d => d.UNIVERSITY).ToList();
            return View(departments);
        }

        // Show Active Schools
        public ActionResult ViewActiveSchools()
        {
            var departments = _db.DEPARTMENTs.Include(d => d.UNIVERSITY).Where(d => d.IsActive == true).ToList();
            return View("ViewSchools", departments); // Return same View with filtered data
        }

        // Show Deactivated Schools
        public ActionResult ViewDeactivatedSchools()
        {
            var departments = _db.DEPARTMENTs.Include(d => d.UNIVERSITY).Where(d => d.IsActive == false).ToList();
            return View("ViewSchools", departments); // Return same View with filtered data
        }


        // Show Statistics Page
        /* public ActionResult Statistics()
         {
             // Get universities along with their departments
             var universities = _db.UNIVERSITies
                                    .Include(u => u.DEPARTMENTs)  // Include departments for each university
                                    .ToList();

             return View(universities);  // Passing universities with departments to the view
         }*/

        // ✅ Show All Universities & Their Schools
        public ActionResult ViewAllUniversitiesAndSchools()
        {
            var universities = _db.UNIVERSITies.Include(u => u.DEPARTMENTs).ToList();
            return View("ViewAllUniversitiesAndSchools", universities);
        }

        // ✅ Show Only Active Universities & Schools
        public ActionResult ViewActiveUniversitiesAndSchools()
        {
            var universities = _db.UNIVERSITies
                .Include(u => u.DEPARTMENTs)
                .Where(u => u.IsActive == true)
                .ToList();
            return View("ViewAllUniversitiesAndSchools", universities);
        }

        // ✅ Show Only Deactivated Universities & Schools
        public ActionResult ViewDeactivatedUniversitiesAndSchools()
        {
            var universities = _db.UNIVERSITies
                .Include(u => u.DEPARTMENTs)
                .Where(u => u.IsActive == false)
                .ToList();
            return View("ViewAllUniversitiesAndSchools", universities);
        }


        //addmentor
        [HttpGet]
        public ActionResult AddMentor()
        {
            try
            {
                // Fetch active universities and store in ViewBag
                ViewBag.Universities = new SelectList(_db.UNIVERSITies
                    .Where(u => (bool)u.IsActive)
                    .Select(u => new { u.UniversityID, u.UniversityNAME }),
                    "UniversityID", "UniversityNAME");

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading universities: " + ex.Message;
                return View();
            }
        }
        [HttpGet]
        public JsonResult GetActiveDepartments(int universityId)
        {
            var activeDepartments = _db.Set<DEPARTMENT>()
                .Where(d => d.Universityid == universityId && d.IsActive == true)

                .Select(d => new { d.DepartmentID, d.DepartmentName })
                .ToList();

            return Json(activeDepartments, JsonRequestBehavior.AllowGet);
        }


        //addmentor

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddMentor(USER model, HttpPostedFileBase Photo, int UniversityID)
        {
            // Reload universities dropdown to avoid null error
            ViewBag.Universities = new SelectList(_db.UNIVERSITies
                .Where(u => (bool)u.IsActive)
                .Select(u => new { u.UniversityID, u.UniversityNAME }),
                "UniversityID", "UniversityNAME");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Upload photo if provided
                if (Photo != null && Photo.ContentLength > 0)
                {
                    string uploadDir = Server.MapPath("~/Uploads/");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string fileName = Path.GetFileName(Photo.FileName);
                    string path = Path.Combine(uploadDir, fileName);
                    Photo.SaveAs(path);
                    model.PhotoPath = "~/Uploads/" + fileName;
                }

                // Set registration and activation date
                model.RegistrationDate = DateTime.Now;
                model.IsActiveDate = DateTime.Now;

                // Ensure mentor is active when added
                model.IsActive = true;  // ✅ Fix: Set IsActive to true by default

                // Set user role as Mentor
                model.Userrole = "Mentor";

                // Save mentor details in USER table
                _db.USERs.Add(model);
                await _db.SaveChangesAsync();

                // Store login credentials in Logins table
                var login = new Models.Login
                {
                    Email = model.Email,
                    PasswordHash = "Mentor@123", // TODO: Hash this in production
                    Role = "Mentor",
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    UniversityID = UniversityID // Store the university ID here
                };

                _db.Logins.Add(login);
                await _db.SaveChangesAsync();

                // Send welcome email
                string subject = "Welcome to Our Platform!";
                string body = $"Hello {model.FirstName},<br/><br/>" +
                              $"You have been successfully added as a mentor. Here are your login details:<br/>" +
                              $"<strong>Username:</strong> {model.Email}<br/>" +
                              $"<strong>Password:</strong> Mentor@123 (Please change your password upon login).<br/><br/>" +
                              "Please log in and complete your profile.";

                // Send email asynchronously
                await _emailService.SendEmailAsync(model.Email, subject, body);

                // Store success message in TempData to persist through redirect
                TempData["SuccessMessage"] = "Mentor added successfully.";

                // Redirect to avoid resubmission
                return RedirectToAction("AddMentor");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
                return View(model);
            }
        }



        // viewmentors
        public ActionResult ViewMentors()
        {
            var universities = _db.USERs.ToList();
            return View(universities);
        }

        public ActionResult ViewActiveMentors()
        {
            var universities = _db.USERs.Where(u => u.IsActive == true).ToList();
            return View("ViewMentors", universities);
        }

        public ActionResult ViewDeactivatedMentors()
        {
            var universities = _db.USERs.Where(u => u.IsActive == false).ToList();
            return View("ViewMentors", universities);
        }



        // 1. Manage Mentors (View and Actions)
        // GET: Manage Mentors Page
        public ActionResult ManageMentors()
        {
            var mentors = _db.USERs.Where(u => u.Userrole == "Mentor").ToList(); // Get all mentors from the USER table
            return View(mentors);
        }

        // POST: Deactivate Mentor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeactivateMentor(int mentorId)
        {
            var mentor = _db.USERs.FirstOrDefault(m => m.UserID == mentorId);
            if (mentor != null)
            {
                mentor.IsActive = false; // Set IsActive to false
                mentor.IsActiveDate = DateTime.Now; // Set deactivation timestamp
                _db.SaveChanges(); // Save changes to database

                TempData["SuccessMessage"] = "Mentor has been deactivated successfully."; // Success message
            }
            else
            {
                TempData["ErrorMessage"] = "Mentor not found."; // Error message if mentor is not found
            }

            return RedirectToAction("ManageMentors"); // Redirect back to ManageMentors page
        }

        // POST: Activate Mentor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivateMentor(int mentorId)
        {
            var mentor = _db.USERs.FirstOrDefault(m => m.UserID == mentorId);
            if (mentor != null)
            {
                mentor.IsActive = true; // Set IsActive to true
                mentor.IsActiveDate = DateTime.Now; // Set activation timestamp
                _db.SaveChanges(); // Save changes to database

                TempData["SuccessMessage"] = "Mentor has been activated successfully."; // Success message
            }
            else
            {
                TempData["ErrorMessage"] = "Mentor not found."; // Error message if mentor is not found
            }

            return RedirectToAction("ManageMentors"); // Redirect back to ManageMentors page
        }


        // Club Requests (for approving or rejecting clubs)
        public ActionResult ClubRequests()
        {
            var clubs = _db.CLUBS
                .Include(c => c.Login)  // ✅ Fetch Mentor Email from Logins Table
                .Include(c => c.DEPARTMENT.UNIVERSITY)  // ✅ Load University & Department
                .ToList();

            // ✅ Fetch Mentor Names from USERs Table Using Email
            foreach (var club in clubs)
            {
                var mentorUser = _db.USERs.FirstOrDefault(u => u.Email == club.Login.Email);
                club.MentorName = mentorUser != null ? mentorUser.FirstName + " " + mentorUser.LastName : "Not Assigned";
            }

            return View(clubs);
        }


        // Approve Club
        //public ActionResult ApproveClub(int id)
        //{
        //    var club = _db.CLUBS.Find(id);
        //    if (club != null)
        //    {
        //        club.ApprovalStatusID = 2; // 2 means 'APPROVED'
        //        club.IsActive = true;
        //        _db.SaveChanges();
        //    }

        //    return RedirectToAction("ClubRequests");
        //}


        public ActionResult ApproveClub(int id)
        {
            var club = _db.CLUBS.Find(id);
            if (club != null)
            {
                club.ApprovalStatusID = 2; // Approved
                _db.SaveChanges();

                // ✅ Create a notification with Start & End Date
                var notification = new Notification
                {
                    LoginID = club.MentorID,  // Mentor's LoginID
                    Message = $"Your club '{club.ClubName}' has been approved!",
                    IsRead = false,  // Unread by default
                    StartDate = DateTime.Now,  // Starts now
                    EndDate = DateTime.Now.AddDays(7),  // Expires in 7 days
                    CreatedDate = DateTime.Now
                };

                _db.Notifications.Add(notification);
                _db.SaveChanges();
            }

            return RedirectToAction("ClubRequests");
        }


        // ❌ Reject Club with Reason
        [HttpPost]
        public ActionResult RejectClub(int id, string reason)
        {
            var club = _db.CLUBS.Find(id);

            if (club != null)
            {
                // ❌ Update Status to 'Rejected'
                club.ApprovalStatusID = 3; // 3 = Rejected
                _db.SaveChanges();

                // ✅ Ensure reason is not empty
                if (string.IsNullOrWhiteSpace(reason))
                {
                    reason = "No specific reason provided."; // Default message
                }

                // ✅ Debug: Check if Mentor ID exists
                if (club.MentorID == null)
                {
                    System.Diagnostics.Debug.WriteLine("🚨 Error: MentorID is NULL for club ID: " + id);
                    TempData["ErrorMessage"] = "Mentor ID not found for this club!";
                    return RedirectToAction("ClubRequests");
                }

                // ✅ Create a notification for the mentor
                var notification = new Notification
                {
                    LoginID = club.MentorID, // Mentor's LoginID
                    Message = $"❌ Your club '{club.ClubName}' was rejected.\nReason: {reason}",
                    IsRead = false, // Mark as unread
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(7), // Notification expires in 7 days
                    CreatedDate = DateTime.Now
                };

                _db.Notifications.Add(notification);
                int changes = _db.SaveChanges(); // Save notification

                // ✅ Debug: Check if notification was added successfully
                if (changes > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Notification added successfully for MentorID: {club.MentorID} with reason: {reason}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("🚨 Error: Notification NOT added!");
                }

                TempData["SuccessMessage"] = $"Club '{club.ClubName}' rejected successfully!";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🚨 Error: Club not found for ID: " + id);
                TempData["ErrorMessage"] = "Club not found!";
            }

            return RedirectToAction("ClubRequests");
        }

        public ActionResult ClubStatus()
        {
            var clubss = _db.CLUBS.Include(c => c.DEPARTMENT)
                                 .Include(c => c.DEPARTMENT.UNIVERSITY)
                                 .ToList();

            var mentorIds = clubss.Select(c => c.MentorID).ToList();


            var notifications = _db.Notifications
                                   .Where(n => n.LoginID.HasValue && mentorIds.Contains(n.LoginID.Value) && n.Message.Contains("rejected"))
                                   .ToList();

            var clubs = _db.CLUBS
               .Include(c => c.DEPARTMENT)
               .Include(c => c.DEPARTMENT.UNIVERSITY)
               .Include(c => c.ApprovalStatusTable)
               .Include(c => c.Login) // Ensure Login table is included
               .ToList();

            // Fetch Mentor Names using Email from Users table
            var mentorEmails = clubs.Select(c => c.Login.Email).Distinct().ToList();
            var mentorNames = _db.USERs
                .Where(u => mentorEmails.Contains(u.Email))
                .ToDictionary(u => u.Email, u => u.FirstName);

            // Assign Mentor Name to each club
            foreach (var club in clubs)
            {
                if (!string.IsNullOrEmpty(club.Login?.Email) && mentorNames.ContainsKey(club.Login.Email))
                {
                    club.MentorName = mentorNames[club.Login.Email]; // Assign Name
                }
                else
                {
                    club.MentorName = "Unknown Mentor"; // Fallback if no mentor found
                }
            }


            ViewBag.Notifications = notifications;
            return View(clubs);
        }
        //public ActionResult ClubStatus()
        //{
        //    var clubs = _db.CLUBS
        //        .Include(c => c.DEPARTMENT)
        //        .Include(c => c.DEPARTMENT.UNIVERSITY)
        //        .Include(c => c.ApprovalStatusTable)
        //        .Include(c => c.Login) // Ensure Login table is included
        //        .ToList();

        //    // Fetch Mentor Names using Email from Users table
        //    var mentorEmails = clubs.Select(c => c.Login.Email).Distinct().ToList();
        //    var mentorNames = _db.USERs
        //        .Where(u => mentorEmails.Contains(u.Email))
        //        .ToDictionary(u => u.Email, u => u.FirstName);

        //    // Assign Mentor Name to each club
        //    foreach (var club in clubs)
        //    {
        //        if (!string.IsNullOrEmpty(club.Login?.Email) && mentorNames.ContainsKey(club.Login.Email))
        //        {
        //            club.MentorName = mentorNames[club.Login.Email]; // Assign Name
        //        }
        //        else
        //        {
        //            club.MentorName = "Unknown Mentor"; // Fallback if no mentor found
        //        }
        //    }

        //    return View(clubs);
        //}



        // Manage Clubs (activate or deactivate)
        public ActionResult ManageClubs()
        {
            var clubs = _db.CLUBS.ToList(); // Fetch all clubs
            return View(clubs);
        }

        // Activate Club
        public ActionResult ActivateClub(int id)
        {
            var club = _db.CLUBS.Find(id);
            if (club != null)
            {
                club.IsActive = true; // Set club to active
                _db.SaveChanges();
            }

            return RedirectToAction("ManageClubs");
        }

        // Deactivate Club
        public ActionResult DeactivateClub(int id)
        {
            var club = _db.CLUBS.Find(id);
            if (club != null)
            {
                club.IsActive = false; // Set club to inactive
                _db.SaveChanges();
            }

            return RedirectToAction("ManageClubs");
        }

        // View All Universities (for "View More" section)
        public ActionResult ViewAllUniversities()
        {
            var universities = _db.UNIVERSITies.ToList();
            return View(universities); // Pass universities to the view
        }


    }
}