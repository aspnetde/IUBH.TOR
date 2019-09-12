const bodyParser = require("body-parser");
const express = require("express");
const path = require("path");
const sleep = require("sleep");

const app = express();
const port = 80;

const username = "test.user";
const password = "test123";

app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

app.post("/login", function(request, response) {
  sleep.sleep(2);

  let isCredentialsValid =
    request.body.user === username && request.body.password === password;

  if (isCredentialsValid) {
    response.send("Welcome.");
    return;
  }

  response.send("Login credentials incorrect!");
});

app.get("/tor", (_, response) => {
  sleep.sleep(3);
  response.sendFile(path.join(__dirname + "/../../testdata/demo-tor.html"));
});

app.listen(port, () =>
  console.log("iubh.tor.server listening on port " + port)
);
