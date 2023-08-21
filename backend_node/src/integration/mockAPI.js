import axios from "axios";

async function fetchMockAPIContacts() {
    //const contacts = await axios.get('https://613b9035110e000017a456b1.mockapi.io/api/v1/contacts').then((response) => response.data);
    const contacts = await axios.get('http://localhost:5000/newmockapicontacts').then((response) => response.data);
    if (!contacts) return [];
    return contacts;
};

module.exports = { fetchMockAPIContacts };