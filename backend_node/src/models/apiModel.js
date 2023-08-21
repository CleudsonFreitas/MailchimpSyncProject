import mongoose from "mongoose";

const Schema = mongoose.Schema;

export const ContactSchema = new Schema({
    id: {
        type: String,
        required: 'Enter a contact Id'
    },
    firstName: {
        type: String,
        required: 'Enter a first name'
    },
    lastName: {
        type: String,
        required: 'Enter a last name'
    },
    email: {
        type: String,
        required: 'Enter a e-mail'
    },
    avatar: {
        type: String,
        required: 'Enter a avatar'
    },
    createdAt: {
        type: Date,
        required: 'Enter date of creation'
    },
    mailChimpMemberId: {
        type: String,
        default: " "
    }
}, {
    toObject: {
        transform: function (doc,ret) {
            delete ret._id,
            delete ret.__v
        }
    }
});
