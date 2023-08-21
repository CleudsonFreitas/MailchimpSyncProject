import { ContactSchema } from '../models/apiModel';
import { addMailChimpMember, removeMailChimpMember } from '../integration/mailChimp';
import { fetchMockAPIContacts } from '../integration/mockAPI';
import mongoose from 'mongoose';
const Contact = mongoose.model('Contact', ContactSchema);

export const getAllContacts = (req, res) => {
    
    Contact.find({}, (err, contact) => {
        if(err) {
            res.status(500).send(err);
        }

        res.status(200).send(contact);
    });

};

export const getContactById = (req, res) => {

    Contact.find({"id": req.params.contactId}, (err, contact) => {
        if(err) {
            res.status(500).send(err);
        }

        if(contact.length === 0)
        {
            res.status(404).json({message: `Contact with ID ${req.params.contactId} not found.`});
        }else
        {
            res.status(200).send(contact);
        }
    });
};

export const syncAllContacts = (req, res) => {

    Contact.find({}).
    then(
        async (contacts) => 
        { 
            let mockAPIContacts = await fetchMockAPIContacts();
            mockAPIContacts = mockAPIContacts.filter((e) => !contacts.find(rm => (rm.id === e.id)));

            mockAPIContacts.forEach(async (c) => {
        
                const mailChimpMember = await addMailChimpMember(c);
                
                if(!mailChimpMember)
                    console.log(`Contact ${c.email} not added! MailChimp integration problem!`);
                else
                {
                    var newContact = new Contact(c);
                    newContact.mailChimpMemberId = mailChimpMember.id;
            
                    newContact.save((err, contact) => {
                        if(err) {
                            res.status(500).send(err);
                        }
                    });
                }
            });

            res.status(200).send(mockAPIContacts);
        }
    );
};

export const deleteContact = (req, res) => {

    Contact.find({"id": req.params.contactId}).
    then(
        async (contacts) => 
        { 
            if(!contacts[0])
            {
                res.status(404).json({message: `Contact with ID ${req.params.contactId} not found.`});
            }
            else
            {
                const isRemoved = await removeMailChimpMember(contacts[0].mailChimpMemberId);
                if(isRemoved)
                {
                    Contact.remove({"id": req.params.contactId}, (err) => {
                        if(err) {
                            res.status(500).send(err);
                        }
                        res.status(200).json({message: `Contact id ${contacts[0].id} deleted from mailChimp and local database successfull`});
                    });
                }
                else
                {
                    res.status(500).json({message: 'Fail on Mailchimp Integration, contact not deleted!'});
                }
            }
        });
};

export const addContact = (req, res) => {
    let newContact = new Contact(req.body);

    newContact.save((err, contact) => {
        
        if(err) {
            res.status(500).send(err);
        }

        res.status(200).send(contact);
    });
};

export const updateContact = (req, res) => {
    
    Contact.findOneAndUpdate({"id": req.params.contactId}, req.body, {new: true }, (err, contact) => {
        if(err) {
            res.status(500).send(err);
        }

        res.status(200).send(contact);
    });   

};
