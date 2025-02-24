﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Win32;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class MentorController : Controller
    {
        private readonly dummyclubsEntities _db = new dummyclubsEntities();

        // ✅ Mentor Dashboard
        public ActionResult Index()
        {
            if (!IsMentorLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            int mentorID = GetMentorID();  // ✅ Fetch Mentor ID

            // ✅ Debug: Check if Mentor ID is retrieved correctly
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Mentor LoginID: {mentorID}");

            // Fetch mentor details
            var mentor = _db.Logins.FirstOrDefault(m => m.LoginID == mentorID && m.Role == "Mentor");
            if (mentor == null)
            {
                TempData["ErrorMessage"] = "Mentor not found!";
                return RedirectToAction("Login", "Admin");
            }

            // Fetch university details
            var university = _db.UNIVERSITies.FirstOrDefault(u => u.UniversityID == mentor.UniversityID);
            if (university == null)
            {
                TempData["ErrorMessage"] = "University not assigned!";
                return RedirectToAction("Login", "Admin");
            }

            // ✅ Debug: Check if university data is retrieved
            System.Diagnostics.Debug.WriteLine($"[DEBUG] University ID: {university.UniversityID}, Name: {university.UniversityNAME}");

            // Fetch notifications
            var notifications = _db.Notifications
                                   .Where(n => n.LoginID == mentorID && n.IsRead == false && n.EndDate > DateTime.Now)
                                   .ToList();

            // ✅ Debug: Check if notifications are retrieved
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Total Unread Notifications: {notifications.Count}");

            // Pass data to the view
            ViewBag.Mentor = mentor;
            ViewBag.University = university;
            ViewBag.Notifications = notifications;

            return View();
        }


        // ✅ Club Registration (Mentors can register clubs under their university)
        //public ActionResult RegisterClub()
        //{
        //    if (!IsMentorLoggedIn())
        //    {
        //        return RedirectToAction("Login", "Admin");
        //    }

        //    return View(new CLUB()); // Create new club model
        //}

        //[HttpPost]
        //public ActionResult RegisterClub(CLUB club)
        //{
        //    if (!IsMentorLoggedIn())
        //    {
        //        return RedirectToAction("Login", "Admin");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.ErrorMessage = "Invalid input. Please fill all required fields.";
        //        return View(club);
        //    }

        //    try
        //    {
        //        int universityID = GetUniversityID();
        //        int mentorID = GetMentorID();

        //        club.UniversityID = universityID;
        //        club.MentorID = mentorID;
        //        club.CreatedDate = DateTime.Now;
        //        club.IsActive = true;

        //        _db.CLUBs.Add(club);
        //        _db.SaveChanges();

        //        TempData["SuccessMessage"] = "Club registered successfully!";
        //        return RedirectToAction("ManageClubs");
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error in RegisterClub: {ex.Message}");
        //        ViewBag.ErrorMessage = "An error occurred while registering the club.";
        //        return View(club);
        //    }
        //}

        //// ✅ Manage Clubs (Mentors can view clubs they registered)
        //public ActionResult ManageClubs()
        //{
        //    if (!IsMentorLoggedIn())
        //    {
        //        return RedirectToAction("Login", "Admin");
        //    }

        //    int mentorID = GetMentorID();
        //    var clubs = _db.CLUBs.Where(c => c.MentorID == mentorID).ToList();

        //    return View(clubs);
        //}

        // ✅ Utility: Check if Mentor is Logged In
        private bool IsMentorLoggedIn()
        {
            return Session["UserRole"] != null &&
                   (string)Session["UserRole"] == "Mentor" &&
                   Session["UserID"] != null;
        }

        // ✅ Utility: Get Mentor ID from Session
        private int GetMentorID()
        {
            return Convert.ToInt32(Session["UserID"]);
        }

        // ✅ Utility: Get University ID from Session
        private int GetUniversityID()
        {
            return Convert.ToInt32(Session["UniversityID"]);
        }

        public ActionResult RegisterClub()
        {
            if (!IsMentorLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            // Fetch the mentor's university ID from session
            int universityID = GetUniversityID();
            System.Diagnostics.Debug.WriteLine("University ID: " + universityID); // Debugging universityID

            // Fetch departments that belong to the mentor's university
            var departments = _db.DEPARTMENTs
                .Where(d => d.Universityid == universityID)
                .ToList();

            // Debugging department count
            System.Diagnostics.Debug.WriteLine("Departments Count: " + departments.Count);

            if (departments != null && departments.Any())
            {
                // Pass the departments to the view as a dropdown
                ViewBag.Departments = new SelectList(departments, "DepartmentID", "DepartmentName");
            }
            else
            {
                // In case no departments found, pass an empty list
                ViewBag.Departments = new SelectList(new List<string>());
            }

            return View(new CLUB()); // Return an empty CLUB model
        }

        [HttpPost]

        public ActionResult RegisterClub(CLUB club)
        {
            if (!IsMentorLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            int universityID = GetUniversityID(); // Get university ID from session

            if (!ModelState.IsValid)
            {
                // Re-fetch departments and return the view with an error message
                ViewBag.ErrorMessage = "Invalid input. Please fill all required fields.";
                ViewBag.Departments = new SelectList(_db.DEPARTMENTs
                    .Where(d => d.Universityid == universityID)
                    .ToList(), "DepartmentID", "DepartmentName");

                return View(club);
            }

            try
            {
                int mentorID = GetMentorID(); // Get Mentor ID

                // Set club properties
                club.MentorID = mentorID;
                club.CreatedDate = DateTime.Now;
                club.IsActive = false; // Initially inactive until admin approval

                // ✅ Handle logo file upload
                if (Request.Files["LogoImage"] != null && Request.Files["LogoImage"].ContentLength > 0)
                {
                    var file = Request.Files["LogoImage"];
                    var fileName = Path.GetFileName(file.FileName);

                    // Define the folder path where the file will be stored
                    string uploadFolder = Server.MapPath("~/Uploads");

                    // Create the directory if it does not exist
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // Define the file path inside the Uploads folder
                    var filePath = Path.Combine(uploadFolder, fileName);
                    file.SaveAs(filePath);

                    // Store relative file path in DB
                    club.LogoImagePath = "/Uploads/" + fileName;
                }

                // ✅ Set ApprovalStatusID to 'PENDING'
                club.ApprovalStatusID = _db.ApprovalStatusTables
                    .FirstOrDefault(a => a.Status == "PENDING")?.ApprovalStatusID ?? 1;

                // ✅ Add to the database
                _db.CLUBS.Add(club);
                _db.SaveChanges();

                // ✅ Clear ModelState to reset form fields
                ModelState.Clear();

                // ✅ Provide success message in the same view
                ViewBag.SuccessMessage = "Club registration request sent to admin!";

                // ✅ Refresh departments dropdown
                ViewBag.Departments = new SelectList(_db.DEPARTMENTs
                    .Where(d => d.Universityid == universityID)
                    .ToList(), "DepartmentID", "DepartmentName");

                // ✅ Return the same view with cleared form fields
                return View(new CLUB());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RegisterClub: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while registering the club.";

                // Re-fetch departments if an error occurs
                ViewBag.Departments = new SelectList(_db.DEPARTMENTs
                    .Where(d => d.Universityid == universityID)
                    .ToList(), "DepartmentID", "DepartmentName");

                return View(club);
            }
        }


        private int GetLoginID()
        {
            if (Session["UserID"] != null)
            {
                return Convert.ToInt32(Session["UserID"]); // Return the stored LoginID
            }
            return 0; // Return 0 if no user is logged in
        }





        public ActionResult MarkNotificationAsRead(int notificationId)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MarkNotificationAsRead called with ID: {notificationId}");

            var notification = _db.Notifications.FirstOrDefault(n => n.NotificationID == notificationId);

            if (notification != null)
            {
                notification.IsRead = true;  // ✅ Mark as read
                _db.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Notification {notificationId} marked as read.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Notification {notificationId} NOT FOUND!");
            }

            return RedirectToAction("Index"); // Refresh dashboard
        }






    }
}
