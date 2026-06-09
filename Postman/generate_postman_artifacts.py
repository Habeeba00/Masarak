import json
import os
import uuid

def create_request(name, method, url_path, auth_type=None, auth_token_var=None, body=None, tests=None, prerequest=None):
    req = {
        "name": name,
        "request": {
            "method": method,
            "header": [
                {
                    "key": "Content-Type",
                    "value": "application/json",
                    "type": "text"
                }
            ],
            "url": {
                "raw": "{{baseUrl}}" + url_path,
                "host": ["{{baseUrl}}"],
                "path": url_path.strip("/").split("/")
            }
        },
        "response": [],
        "event": []
    }
    
    if auth_type == "bearer":
        req["request"]["auth"] = {
            "type": "bearer",
            "bearer": [
                {
                    "key": "token",
                    "value": "{{" + auth_token_var + "}}",
                    "type": "string"
                }
            ]
        }

    if body:
        req["request"]["body"] = {
            "mode": "raw",
            "raw": json.dumps(body, indent=4)
        }
        
    if prerequest:
        req["event"].append({
            "listen": "prerequest",
            "script": {
                "exec": prerequest,
                "type": "text/javascript"
            }
        })

    if tests:
        req["event"].append({
            "listen": "test",
            "script": {
                "exec": tests,
                "type": "text/javascript"
            }
        })
        
    # Remove event array if empty
    if not req["event"]:
        del req["event"]

    return req


def get_login_tests(role_prefix):
    return [
        'pm.test("Status code is 200", function () {',
        '    pm.response.to.have.status(200);',
        '});',
        'var jsonData = pm.response.json();',
        'pm.test("Response has accessToken and refreshToken", function () {',
        '    pm.expect(jsonData.accessToken).to.exist;',
        '    pm.expect(jsonData.refreshToken).to.exist;',
        '});',
        'if (jsonData.accessToken) {',
        f'    pm.environment.set("{role_prefix}AccessToken", jsonData.accessToken);',
        f'    pm.environment.set("{role_prefix}RefreshToken", jsonData.refreshToken);',
        '}',
        'pm.test("JWT has 3 parts", function () {',
        '    let parts = jsonData.accessToken.split(".");',
        '    pm.expect(parts.length).to.eql(3);',
        '});',
        'pm.test("JWT contains required claims", function () {',
        '    let payload = JSON.parse(atob(jsonData.accessToken.split(".")[1]));',
        '    pm.expect(payload.role).to.exist;',
        '    pm.expect(payload.userid).to.exist;',
        '    pm.expect(payload.email).to.exist;',
        '    pm.expect(payload.exp).to.exist;',
        '    pm.expect(payload.exp * 1000).to.be.above(Date.now());',
        '});'
    ]

def get_status_test(code):
    return [
        f'pm.test("Status code is {code}", function () {{',
        f'    pm.response.to.have.status({code});',
        '});'
    ]

def get_register_tests():
    return [
        'pm.test("Registration successful or user already exists", function () {',
        '    pm.expect(pm.response.code).to.be.oneOf([200, 400]);',
        '});'
    ]

# Collection Structure
collection = {
    "info": {
        "name": "Masarak_Phase2",
        "description": "Comprehensive Postman testing suite for Masarak Education API - Phase 2 (Authentication & Authorization).",
        "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    },
    "item": [
        {
            "name": "Authentication",
            "item": [
                create_request("Register Admin", "POST", "/api/auth/register", body={
                    "fullName": "System Administrator", "email": "{{adminEmail}}", "password": "{{adminPassword}}", "confirmPassword": "{{adminPassword}}", "phone": "+201001234560", "country": "EG", "role": "Admin"
                }, prerequest=["let ts = Date.now(); pm.environment.set('adminEmail', `admin_${ts}@masarak.com`);"], tests=get_status_test(200)),
                create_request("Register Teacher", "POST", "/api/auth/register", body={
                    "fullName": "Ahmed Hassan", "email": "{{teacherEmail}}", "password": "{{teacherPassword}}", "confirmPassword": "{{teacherPassword}}", "phone": "+201001234561", "country": "EG", "role": "Teacher"
                }, prerequest=["let ts = Date.now(); pm.environment.set('teacherEmail', `teacher_${ts}@masarak.com`);"], tests=get_status_test(200)),
                create_request("Register Student", "POST", "/api/auth/register", body={
                    "fullName": "Ali Student", "email": "{{studentEmail}}", "password": "{{studentPassword}}", "confirmPassword": "{{studentPassword}}", "phone": "+201001234562", "country": "EG", "role": "Student"
                }, prerequest=["let ts = Date.now(); pm.environment.set('studentEmail', `student_${ts}@masarak.com`);"], tests=get_status_test(200)),
                create_request("Register Parent", "POST", "/api/auth/register", body={
                    "fullName": "Hassan Parent", "email": "{{parentEmail}}", "password": "{{parentPassword}}", "confirmPassword": "{{parentPassword}}", "phone": "+201001234563", "country": "EG", "role": "Parent"
                }, prerequest=["let ts = Date.now(); pm.environment.set('parentEmail', `parent_${ts}@masarak.com`);"], tests=get_status_test(200)),
                
                create_request("Login Admin", "POST", "/api/auth/login", body={
                    "email": "{{adminEmail}}", "password": "{{adminPassword}}"
                }, tests=get_login_tests("admin")),
                create_request("Login Teacher", "POST", "/api/auth/login", body={
                    "email": "{{teacherEmail}}", "password": "{{teacherPassword}}"
                }, tests=get_login_tests("teacher")),
                create_request("Login Student", "POST", "/api/auth/login", body={
                    "email": "{{studentEmail}}", "password": "{{studentPassword}}"
                }, tests=get_login_tests("student")),
                create_request("Login Parent", "POST", "/api/auth/login", body={
                    "email": "{{parentEmail}}", "password": "{{parentPassword}}"
                }, tests=get_login_tests("parent")),
                
                create_request("Get Current User", "GET", "/api/auth/me", auth_type="bearer", auth_token_var="adminAccessToken", tests=get_status_test(200)),
                
                create_request("Refresh Token", "POST", "/api/auth/refresh", body={
                    "accessToken": "{{adminAccessToken}}", "refreshToken": "{{adminRefreshToken}}"
                }, tests=get_login_tests("admin")),
                
                create_request("Change Password", "POST", "/api/auth/change-password", auth_type="bearer", auth_token_var="adminAccessToken", body={
                    "currentPassword": "{{adminPassword}}", "newPassword": "Admin@NewPass99!", "confirmNewPassword": "Admin@NewPass99!"
                }, tests=get_status_test(200)),
                
                create_request("Logout", "POST", "/api/auth/logout", body={
                    "refreshToken": "{{adminRefreshToken}}"
                }, tests=get_status_test(200)),
                
                create_request("Revoke Token", "POST", "/api/auth/revoke-token", auth_type="bearer", auth_token_var="adminAccessToken", body={
                    "refreshToken": "{{teacherRefreshToken}}"
                }, tests=get_status_test(200))
            ]
        },
        {
            "name": "Admin",
            "item": [
                create_request("Get Dashboard", "GET", "/api/admin/dashboard", auth_type="bearer", auth_token_var="adminAccessToken", tests=get_status_test(200)),
                create_request("Get Users", "GET", "/api/admin/users", auth_type="bearer", auth_token_var="adminAccessToken", tests=get_status_test(200)),
                create_request("Delete User", "DELETE", "/api/admin/users/999", auth_type="bearer", auth_token_var="adminAccessToken", tests=get_status_test(200))
            ]
        },
        {
            "name": "Teacher",
            "item": [
                create_request("Get Classes", "GET", "/api/teacher/classes", auth_type="bearer", auth_token_var="teacherAccessToken", tests=get_status_test(200)),
                create_request("Get Assignments", "GET", "/api/teacher/assignments", auth_type="bearer", auth_token_var="teacherAccessToken", tests=get_status_test(200)),
                create_request("Create Session", "POST", "/api/teacher/sessions", auth_type="bearer", auth_token_var="teacherAccessToken", body={"title":"New Session"}, tests=get_status_test(200)),
                create_request("Grade Student", "POST", "/api/teacher/grade/1", auth_type="bearer", auth_token_var="teacherAccessToken", body=95, tests=get_status_test(200))
            ]
        },
        {
            "name": "Student",
            "item": [
                create_request("Get Courses", "GET", "/api/student/courses", auth_type="bearer", auth_token_var="studentAccessToken", tests=get_status_test(200)),
                create_request("Get Grades", "GET", "/api/student/grades", auth_type="bearer", auth_token_var="studentAccessToken", tests=get_status_test(200)),
                create_request("Submit Assignment", "POST", "/api/student/assignments/1/submit", auth_type="bearer", auth_token_var="studentAccessToken", body={"content":"My work"}, tests=get_status_test(200)),
                create_request("Get Exams", "GET", "/api/student/exams", auth_type="bearer", auth_token_var="studentAccessToken", tests=get_status_test(200))
            ]
        },
        {
            "name": "Parent",
            "item": [
                create_request("Get Children", "GET", "/api/parent/children", auth_type="bearer", auth_token_var="parentAccessToken", tests=get_status_test(200)),
                create_request("Get Child Grades", "GET", "/api/parent/children/1/grades", auth_type="bearer", auth_token_var="parentAccessToken", tests=get_status_test(200)),
                create_request("Get Child Attendance", "GET", "/api/parent/children/1/attendance", auth_type="bearer", auth_token_var="parentAccessToken", tests=get_status_test(200))
            ]
        },
        {
            "name": "Shared",
            "item": [
                create_request("Get Progress", "GET", "/api/shared/progress/1", auth_type="bearer", auth_token_var="studentAccessToken", tests=get_status_test(200)),
                create_request("Get Analytics", "GET", "/api/shared/analytics", auth_type="bearer", auth_token_var="teacherAccessToken", tests=get_status_test(200)),
                create_request("Get Notifications", "GET", "/api/shared/notifications", auth_type="bearer", auth_token_var="studentAccessToken", tests=get_status_test(200))
            ]
        },
        {
            "name": "Negative Tests",
            "item": [
                create_request("Expired JWT", "GET", "/api/auth/me", auth_type="bearer", auth_token_var="expiredToken", tests=get_status_test(401)),
                create_request("Invalid JWT", "GET", "/api/auth/me", auth_type="bearer", auth_token_var="invalidToken", tests=get_status_test(401)),
                create_request("Missing JWT", "GET", "/api/auth/me", tests=get_status_test(401)),
                create_request("Wrong Role Access (Teacher -> Admin)", "GET", "/api/admin/dashboard", auth_type="bearer", auth_token_var="teacherAccessToken", tests=get_status_test(403)),
                create_request("Reused Refresh Token", "POST", "/api/auth/refresh", body={
                    "accessToken": "{{adminAccessToken}}", "refreshToken": "{{usedRefreshToken}}"
                }, tests=get_status_test(400)),
                create_request("Revoked Refresh Token", "POST", "/api/auth/refresh", body={
                    "accessToken": "{{adminAccessToken}}", "refreshToken": "{{revokedRefreshToken}}"
                }, tests=get_status_test(400)),
                create_request("Locked User Login", "POST", "/api/auth/login", body={
                    "email": "{{adminEmail}}", "password": "WrongPassword!"
                }, tests=get_status_test(401))
            ]
        }
    ]
}

environment = {
    "id": str(uuid.uuid4()),
    "name": "Masarak_Local",
    "values": [
        {"key": "baseUrl", "value": "http://localhost:5000", "type": "default", "enabled": True},
        {"key": "adminEmail", "value": "admin@masarak.com", "type": "default", "enabled": True},
        {"key": "adminPassword", "value": "Admin@12345!", "type": "default", "enabled": True},
        {"key": "teacherEmail", "value": "teacher@masarak.com", "type": "default", "enabled": True},
        {"key": "teacherPassword", "value": "Teacher@12345!", "type": "default", "enabled": True},
        {"key": "studentEmail", "value": "student@masarak.com", "type": "default", "enabled": True},
        {"key": "studentPassword", "value": "Student@12345!", "type": "default", "enabled": True},
        {"key": "parentEmail", "value": "parent@masarak.com", "type": "default", "enabled": True},
        {"key": "parentPassword", "value": "Parent@12345!", "type": "default", "enabled": True},
        {"key": "adminAccessToken", "value": "", "type": "default", "enabled": True},
        {"key": "teacherAccessToken", "value": "", "type": "default", "enabled": True},
        {"key": "studentAccessToken", "value": "", "type": "default", "enabled": True},
        {"key": "parentAccessToken", "value": "", "type": "default", "enabled": True},
        {"key": "adminRefreshToken", "value": "", "type": "default", "enabled": True},
        {"key": "teacherRefreshToken", "value": "", "type": "default", "enabled": True},
        {"key": "studentRefreshToken", "value": "", "type": "default", "enabled": True},
        {"key": "parentRefreshToken", "value": "", "type": "default", "enabled": True},
        {"key": "userId", "value": "", "type": "default", "enabled": True},
        {"key": "expiredToken", "value": "eyJhbGciOiJIUzI1NiIsInR5cCI... (paste expired)", "type": "default", "enabled": True},
        {"key": "invalidToken", "value": "invalid.token.string", "type": "default", "enabled": True},
        {"key": "usedRefreshToken", "value": "base64-used-token...", "type": "default", "enabled": True},
        {"key": "revokedRefreshToken", "value": "base64-revoked-token...", "type": "default", "enabled": True}
    ],
    "_postman_variable_scope": "environment"
}

with open("d:/ITI/GradProj/Masarak/Postman/Masarak_Phase2.postman_collection.json", "w") as f:
    json.dump(collection, f, indent=4)

with open("d:/ITI/GradProj/Masarak/Postman/Masarak_Local.postman_environment.json", "w") as f:
    json.dump(environment, f, indent=4)

print("Files generated.")
