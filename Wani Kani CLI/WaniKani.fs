module WaniKani

open canopy.runner.classic
open canopy.configuration
open canopy.classic
open Config

let login creds = 
    url "https://www.wanikani.com/login"
    "#user_login" << creds.username
    "#user_password" << creds.password
    click "html body section.session.login div.wrapper form#new_user.new_user fieldset button.button"
    read "html#main body div.footer-adjustment div.navbar.navbar-static-top div.navbar-inner div.container ul.nav li.reviews.wanikani-tour-3 a span"