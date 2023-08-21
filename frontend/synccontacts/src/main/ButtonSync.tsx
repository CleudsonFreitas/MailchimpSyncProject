import React, {useState, useCallback, useEffect} from 'react'
import { Contact } from '../types/contact';
import configApp from '../configApp';
import axios from 'axios';
import "./ButtonSync.css";

const ButtonSync = () => {

  const [contacts, setContacts] = useState<Contact[]>([]);
  const [isSending, setIsSending] = useState(false);
  const [textSync, setTextSync] = useState("");

  const sendRequest = useCallback(async () => {
    setIsSending(true);
    const contacts = await axios.get<Contact[]>(`${configApp.baseApiUrl}/contacts/sync`).then((response) => response.data);
    setContacts(contacts);
    setIsSending(false);
  }, [isSending]);

  useEffect(() => {
    if(isSending === true) {
      setTextSync("Synchronizing contacts, wait...");
    } 
    else if((isSending === false) && (contacts.length > 0)) {
      setTextSync(`contacts were synced!`);
    }
    else if((isSending === false) && (contacts.length === 0)) {
      setTextSync("No contacts to sync, everything is updated!");
    }
  }, [isSending]);

  useEffect(() => {
    setTextSync("Sync Contacts");
  }, []);

  return (
    <>
      <div className={isSending ? "button center rotate_minus180" : "button center rotate_zero"} onClick={sendRequest}>
        <img src="arrows.png" className="syncIcon" alt="click to sync"/>
      </div>
      <input type="text" value={textSync} className='text-sync'></input>
    </>
  );
};

export default ButtonSync