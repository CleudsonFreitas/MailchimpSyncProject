import axios from "axios";

async function addMailChimpMember(contact) {
    
    const audienceID = "cc55800fc9";
    const token = "e5dd6580abef369bcd4b6e9acbd5836a-us21";

    let newContact = {
        email_address: contact.email,
        full_name: contact.firstName + ' ' + contact.lastName,
        status: 'subscribed',
        merge_fields: {
            FNAME: contact.firstName,
            LNAME: contact.lastName
       }
    }

    const member = await axios.post(`https://us21.api.mailchimp.com/3.0/lists/${audienceID}/members`, newContact, {
        headers: {
            'Content-Type': 'text/json',
            'Authorization': `Bearer ${token}`
        }
    }).then((response) => response.data)
    .catch((error) => console.log(error)); // Log internal error

    if (!member) return null;
    return member;
};

async function removeMailChimpMember(memberId) {
    
    const audienceID = "cc55800fc9";
    const token = "e5dd6580abef369bcd4b6e9acbd5836a-us21";

    return await axios.delete(`https://us21.api.mailchimp.com/3.0/lists/${audienceID}/members/${memberId}`, {
        headers: {
            'Content-Type': 'text/json',
            'Authorization': `Bearer ${token}`
        }
    }).then(() => true)
    .catch((error) => console.log(error) ); // Log internal error
};

module.exports = { addMailChimpMember, removeMailChimpMember };