package com.iveely.framework.language;

import java.io.*;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.HashMap;

/**
 * Created by Ahmed on 1/16/15.
 */
public class DetectIdentifier {

    /**
     * the total number of trigrams for the document
     */
    private double num = 0;

    /**
     * the trained languages
     */
    private ArrayList<String> languages = new ArrayList<>();

    /**
     * the languages trained trigram data files
     */
    private ArrayList<HashMap<String, Double>> traineddata = new ArrayList<>();

    /**
     * the trigrams of the document we want to identify
     */
    private HashMap<String, Double> trigrams = new HashMap<>();

    /**
     * counters that are used later
     */
    private int ldcount = 0;

    private int resultcount = 0;

    private String folder = "resources\\language";

    private static final DetectIdentifier identify = new DetectIdentifier();

    DetectIdentifier() {
        File folder = new File("resources\\language");
        File[] listOfFiles = folder.listFiles();
        //in the constructor i load all the learned trigram data files and fill there hashmaps in an array
        for (int i = 0; i < listOfFiles.length; i++) {
            if (listOfFiles[i].isFile()) {
                //get the trigram data files
                if (listOfFiles[i].getName().endsWith(".dat")) {
                    //add the trained language to the list of trained languages
                    String language = listOfFiles[i].getName().substring(0, listOfFiles[i].getName().length() - 4);
                    language = firstLetterCaps(language);
                    languages.add(language);
                    try {
                        //read the language data file
                        HashMap<String, Double> languagedf = new HashMap<String, Double>();
                        InputStream fis = new FileInputStream(folder + "\\" + listOfFiles[i].getName());
                        InputStreamReader isr = new InputStreamReader(fis, Charset.forName("UTF-8"));
                        BufferedReader br = new BufferedReader(isr);
                        String line;
                        while ((line = br.readLine()) != null) {
                            // process the line.
                            String[] tokens = line.split("\\s");
                            if (tokens.length == 2) {
                                languagedf.put(tokens[0], Double.valueOf(tokens[1]));
                            }
                        }
                        //after filling the hashmap with the language trigram data file data
                        // add the filled hashmap to the learned languages array
                        traineddata.add(languagedf);
                        br.close();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }
            }
        }
    }

    static public String firstLetterCaps(String data) {
        String firstLetter = data.substring(0, 1).toUpperCase();
        String restLetters = data.substring(1).toLowerCase();
        return firstLetter + restLetters;
    }

    String identifyLanguage(String text) {
        double value;
        ArrayList<Double> result = new ArrayList<Double>(); //storage for the matches with the models
        //check to which language the document belongs to
        createTrigrams(text); //create trigrams of submitted text
        computeProbability(); //compute probabilities

        //the result array in which each element represent the score of one of the trained data files
        for (String language : languages) {
            result.add((double) 0);
        }

        //loop over the sample file trigrams
        for (String trigram : trigrams.keySet()) {
            ldcount = 0;
            //loop on the trained data files trigrams
            for (HashMap<String, Double> tdf : traineddata) {
                //get the current trained data file
                HashMap<String, Double> mytdf = tdf;
                if (mytdf.containsKey(trigram)) {
                    //if the model contains the key, get the deviation
                    value = mytdf.get(trigram) - trigrams.get(trigram);
                    if (value < 0) {
                        value = value * -1;
                    }
                    result.set(ldcount, result.get(ldcount) + value);
                } else {
                    //set the resulting value to 1 which is the maximum deviation
                    result.set(ldcount, result.get(ldcount) + 1);
                }
                ldcount++;
            }
        }

        //identify the language from the result array as i said before each element represent the score of one language
        value = (double) 1.0;
        int element = 0;
        resultcount = 0;
        for (Double x : result) {
            if (value > (double) x / (double) num) {
                value = (double) x / (double) num;
                element = resultcount;

            }
            resultcount++;
        }
        return languages.get(element);
    }

    void createTrigrams(String text) {
        //clean the text from punctuations, new lines, and multiple spaces.
        text = text.replaceAll("[^\\p{L}]", " ");
        text = text.replaceAll("\n", " ");
        text = text.replaceAll("\\s+", " ");

        //create trigrams and count them.
        for (int i = 0; i < text.length() - 2; i++) {
            String trigram = text.substring(i, i + 3);
            num += 1;
            if (trigrams.containsKey(trigram)) {
                trigrams.put(trigram, trigrams.get(trigram) + 1);
            } else {
                trigrams.put(trigram, (double) 1);
            }
        }
    }

    void computeProbability() {
        for (String trigram : trigrams.keySet()) {
            trigrams.put(trigram, (double) trigrams.get(trigram) / (double) num);
        }
    }

    public static String check(String text) {
        return identify.identifyLanguage(text);
    }
}
